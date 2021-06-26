using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections
{
    public class BindableCollectionAdapter : ReadOnlyCollection<object?>, IThreadDispatcherHandler, IValueHolder<Delegate>
    {
        protected WeakListener? Listener;
        internal List<object?>? ResetCache;

        private readonly List<CollectionChangedEvent> _pendingEvents;
        private readonly IThreadDispatcher? _threadDispatcher;

        private Timer? _timer;
        private int _timerVersion;
        private IEnumerable? _collection;
        private List<CollectionChangedEvent>? _eventsCache;
        private ThreadExecutionMode _executionMode;
        private int _suspendCount;
        private int? _batchLimit;
        private int? _batchDelay;

        public BindableCollectionAdapter(IList<object?>? source = null, IThreadDispatcher? threadDispatcher = null)
            : base(source ?? new List<object?>())
        {
            _threadDispatcher = threadDispatcher;
            _pendingEvents = new List<CollectionChangedEvent>();
            _executionMode = ThreadExecutionMode.Main;
            Version = -BoxingExtensions.CacheSize;
        }

        public IEnumerable? Collection
        {
            get => _collection;
            set
            {
                if (!ReferenceEquals(value, _collection))
                    OnCollectionChanged(_collection, value);
            }
        }

        public ThreadExecutionMode ExecutionMode
        {
            get => _executionMode;
            set
            {
                Should.NotBeNull(value, nameof(value));
                if (!value.IsSynchronized)
                    ExceptionManager.ThrowNotSupported(MessageConstant.AdapterSupportsOnlySynchronizedMode);
                _executionMode = value;
            }
        }

        public int BatchLimit
        {
            get => _batchLimit.GetValueOrDefault(CollectionMetadata.BindableCollectionAdapterBatchLimit);
            set => _batchLimit = value;
        }

        public int BatchDelay
        {
            get => _batchDelay.GetValueOrDefault(CollectionMetadata.BindableCollectionAdapterBatchDelay);
            set => _batchDelay = value;
        }

        protected virtual bool IsAlive => true;

        protected int Version { get; private set; }

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        protected virtual void OnCollectionChanged(IEnumerable? oldValue, IEnumerable? newValue, int version) => ResetCache?.Clear();

        protected virtual void AddCollectionListener(IEnumerable collection, int version)
        {
            if (collection is IComponentOwner componentOwner)
            {
                Listener = new WeakListener(this, version, Listener);
                componentOwner.Components.Add(Listener);
            }
            else if (collection is INotifyCollectionChanged notifyCollectionChanged)
            {
                Listener = new WeakListener(this, version, Listener);
                notifyCollectionChanged.CollectionChanged += Listener.OnCollectionChanged;
            }
        }

        protected virtual void RemoveCollectionListener(IEnumerable collection)
        {
            if (Listener == null)
                return;

            if (collection is IComponentOwner componentOwner)
                componentOwner.Components.Remove(Listener);
            else if (collection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged -= Listener.OnCollectionChanged;
        }

        protected virtual IEnumerable<object?> GetCollectionItems(IEnumerable collection) => collection.Decorate();

        protected virtual void OnChanged(object? item, int index, object? args, bool batchUpdate, int version)
        {
        }

        protected virtual void OnAdded(object? item, int index, bool batchUpdate, int version) => Items.Insert(index, item);

        protected virtual void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version) => Items[index] = newItem;

        protected virtual void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            if (Items is ObservableCollection<object?> observableCollection)
                observableCollection.Move(oldIndex, newIndex);
            else
            {
                Items.RemoveAt(oldIndex);
                Items.Insert(newIndex, item);
            }
        }

        protected virtual void OnRemoved(object? item, int index, bool batchUpdate, int version) => Items.RemoveAt(index);

        protected virtual void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
        {
            Items.Clear();
            if (items != null)
                Items.AddRange(items);
        }

        protected virtual void BatchUpdate(List<CollectionChangedEvent> events, int version)
        {
            if (events.Count == 0)
                return;

            var firstEvent = events[0];
            if (events.Count == 1)
            {
                firstEvent.Raise(this, true, version);
                return;
            }

            if (events.Count < BatchLimit && firstEvent.Action != CollectionChangedAction.Reset)
            {
                RaiseBatchUpdate(events, version);
                return;
            }

            int startIndex;
            if (firstEvent.Action == CollectionChangedAction.Reset)
            {
                MugenExtensions.Reset(ref ResetCache, firstEvent.ResetItems);
                startIndex = 1;
            }
            else
            {
                startIndex = 0;
                MugenExtensions.Reset(ref ResetCache, Items);
            }

            for (var i = startIndex; i < events.Count; i++)
                events[i].ApplyToSource(ResetCache);

            OnReset(ResetCache, true, version);
        }

        protected virtual void RaiseBatchUpdate(List<CollectionChangedEvent> events, int version)
        {
            for (var i = 0; i < events.Count; i++)
                events[i].Raise(this, true, version);
        }

        protected virtual bool AddPendingEvent(List<CollectionChangedEvent> pendingEvents, in CollectionChangedEvent e)
        {
            if (e.Action == CollectionChangedAction.Reset)
            {
                pendingEvents.Clear();
                var resetItems = ((IEnumerable<object?>?)e.NewItem)?.ToArray();
                pendingEvents.Add(CollectionChangedEvent.Reset(resetItems == null || resetItems.Length == 0 ? null : resetItems));
                return true;
            }

            pendingEvents.Add(e);
            return true;
        }

        protected virtual List<CollectionChangedEvent> GetPendingEvents(List<CollectionChangedEvent> pendingEvents)
        {
            MugenExtensions.Reset(ref _eventsCache, pendingEvents);
            return _eventsCache;
        }

        protected virtual bool IsChangeEventSupported(object? item, object? args) => CollectionMetadata.ReloadItem.Equals(args);

        protected bool AddEvent(in CollectionChangedEvent collectionChangedEvent, int version)
        {
            if (Version != version)
                return false;

            var inline = ThreadDispatcher.CanExecuteInline(ExecutionMode);
            var endBatch = false;
            lock (_pendingEvents)
            {
                if (Version != version)
                    return false;

                if (_suspendCount != 0 || !inline)
                {
                    if (!AddPendingEvent(_pendingEvents, collectionChangedEvent))
                        return false;

                    inline = false;
                    if (_suspendCount == 0)
                    {
                        ++_suspendCount;
                        endBatch = true;
                    }
                }
            }

            if (inline)
                collectionChangedEvent.Raise(this, false, version);
            else if (endBatch)
                EndBatchUpdate(version);

            return true;
        }

        protected void BeginBatchUpdate(int version)
        {
            if (Version != version)
                return;

            lock (_pendingEvents)
            {
                if (Version == version)
                    ++_suspendCount;
            }
        }

        protected void EndBatchUpdate(int version)
        {
            if (Version != version)
                return;

            lock (_pendingEvents)
            {
                if (Version != version)
                    return;

                if (_suspendCount > 1 || _pendingEvents.Count == 0)
                {
                    if (_suspendCount != 0)
                        --_suspendCount;
                    return;
                }

                if (BatchDelay != 0 && IsResetRequired())
                {
                    _timerVersion = version;
                    _timer ??= WeakTimer.Get(this, adapter => adapter.EndBatchUpdateTimer(), Listener?.Reference);
                    _timer.Change(BatchDelay, Timeout.Infinite);
                    return;
                }
            }

            ThreadDispatcher.Execute(ExecutionMode, this, BoxingExtensions.Box(version));
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, int version)
        {
            if (IsResetEvent(sender, e, out var items))
            {
                AddEvent(CollectionChangedEvent.Reset(items), version);
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems!.Count; i++)
                        AddEvent(CollectionChangedEvent.Add(e.NewItems[i], e.NewStartingIndex + i), version);
                    break;
                case NotifyCollectionChangedAction.Move:
                    AddEvent(CollectionChangedEvent.Move(e.OldItems![0], e.OldStartingIndex, e.NewStartingIndex), version);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems!.Count; i++)
                        AddEvent(CollectionChangedEvent.Remove(e.OldItems[i], e.OldStartingIndex + i), version);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    AddEvent(CollectionChangedEvent.Replace(e.OldItems![0], e.NewItems![0], e.NewStartingIndex), version);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    AddEvent(CollectionChangedEvent.Reset(null), version);
                    break;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    break;
            }
        }

        private bool IsResetRequired()
        {
            if (_pendingEvents.Count == 0)
                return false;
            if (_pendingEvents[0].Action == CollectionChangedAction.Reset)
                return true;
            return _pendingEvents.Count >= BatchLimit;
        }

        private void EndBatchUpdateTimer()
        {
            if (Version != _timerVersion)
                return;

            int version;
            lock (_pendingEvents)
            {
                version = _timerVersion;
                if (Version != version)
                    return;
            }

            ThreadDispatcher.Execute(ExecutionMode, this, BoxingExtensions.Box(version));
        }

        private async void OnCollectionChanged(IEnumerable? oldValue, IEnumerable? newValue)
        {
            await _threadDispatcher.SwitchToBackgroundAsync();

            int version;
            using var oldLock = MugenExtensions.TryLock(oldValue);
            using var newLock = MugenExtensions.TryLock(newValue);
            lock (_pendingEvents)
            {
                version = ++Version;
                _suspendCount = 1;
                if (oldValue != null)
                {
                    RemoveCollectionListener(oldValue);
                    oldLock.Dispose();
                }

                _pendingEvents.Clear();
                _collection = newValue;
                if (newValue == null)
                    AddEvent(CollectionChangedEvent.Reset(null), version);
                else
                {
                    AddCollectionListener(newValue, version);
                    AddEvent(CollectionChangedEvent.Reset(GetCollectionItems(newValue)), version);
                }
            }

            oldLock.Dispose();
            newLock.Dispose();

            await _threadDispatcher.SwitchToAsync(ExecutionMode);
            if (Version == version)
            {
                OnCollectionChanged(oldValue, newValue, version);
                EndBatchUpdate(version);
            }
        }

        private bool IsResetEvent(object sender, NotifyCollectionChangedEventArgs e, [NotNullWhen(true)] out IEnumerable<object?>? items)
        {
            items = null;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    return false;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems!.Count == 1)
                        return false;
                    items = GetCollectionItems((IEnumerable)sender);
                    return true;
                case NotifyCollectionChangedAction.Reset:
                    items = GetCollectionItems((IEnumerable)sender);
                    return items.CountEx() != 0;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    return false;
            }
        }

        void IThreadDispatcherHandler.Execute(object? state)
        {
            var version = (int)state!;
            if (Version != version)
                return;

            List<CollectionChangedEvent> events;
            lock (_pendingEvents)
            {
                if (Version != version || _suspendCount == 0 || --_suspendCount != 0)
                    return;

                events = GetPendingEvents(_pendingEvents);
                _pendingEvents.Clear();
            }

            if (events.Count != 0)
            {
                BatchUpdate(events, version);
                _eventsCache?.Clear();
            }
        }

        protected enum CollectionChangedAction : byte
        {
            Add = 1,
            Move = 2,
            Remove = 3,
            Replace = 4,
            Reset = 5,
            Changed = 6
        }

        [StructLayout(LayoutKind.Auto)]
        protected readonly struct CollectionChangedEvent
        {
            public readonly CollectionChangedAction Action;
            public readonly int NewIndex;
            public readonly object? NewItem;
            public readonly int OldIndex;
            public readonly object? OldItem;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CollectionChangedEvent(CollectionChangedAction action, object? oldItem, object? newItem, int oldIndex, int newIndex)
            {
                Action = action;
                OldItem = oldItem;
                NewItem = newItem;
                OldIndex = oldIndex;
                NewIndex = newIndex;
            }

            public bool IsEmpty => Action == 0;

            public bool IsResetItemsFixed => OldIndex == int.MaxValue;

            public IEnumerable<object?>? ResetItems
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (IEnumerable<object?>?)NewItem;
            }

            public object? ChangedArgs
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => NewItem;
            }

            public void Raise(BindableCollectionAdapter adapter, bool batchUpdate, int version)
            {
                switch (Action)
                {
                    case CollectionChangedAction.Add:
                        adapter.OnAdded(NewItem, NewIndex, batchUpdate, version);
                        break;
                    case CollectionChangedAction.Move:
                        adapter.OnMoved(NewItem, OldIndex, NewIndex, batchUpdate, version);
                        break;
                    case CollectionChangedAction.Remove:
                        adapter.OnRemoved(OldItem, OldIndex, batchUpdate, version);
                        break;
                    case CollectionChangedAction.Replace:
                        adapter.OnReplaced(OldItem, NewItem, OldIndex, batchUpdate, version);
                        break;
                    case CollectionChangedAction.Reset:
                        adapter.OnReset((IEnumerable<object?>?)NewItem, batchUpdate, version);
                        break;
                    case CollectionChangedAction.Changed:
                        adapter.OnChanged(OldItem, OldIndex, NewItem, batchUpdate, version);
                        break;
                    default:
                        ExceptionManager.ThrowEnumOutOfRange(nameof(Action), Action);
                        break;
                }
            }

            public void ApplyToSource(IList<object?> source)
            {
                Should.NotBeNull(source, nameof(source));
                switch (Action)
                {
                    case CollectionChangedAction.Add:
                        source.Insert(NewIndex, NewItem);
                        break;
                    case CollectionChangedAction.Move:
                        source.RemoveAt(OldIndex);
                        source.Insert(NewIndex, NewItem);
                        break;
                    case CollectionChangedAction.Remove:
                        source.RemoveAt(OldIndex);
                        break;
                    case CollectionChangedAction.Replace:
                        source[OldIndex] = NewItem;
                        break;
                    case CollectionChangedAction.Reset:
                        source.Clear();
                        if (NewItem != null)
                            source.AddRange((IEnumerable<object?>)NewItem!);
                        break;
                }
            }

            public void Raise(ref DiffUtil.BatchingListUpdateCallback callback)
            {
                switch (Action)
                {
                    case CollectionChangedAction.Add:
                        callback.OnInserted(NewIndex, NewIndex, 1);
                        break;
                    case CollectionChangedAction.Move:
                        callback.OnMoved(OldIndex, NewIndex, OldIndex, NewIndex);
                        break;
                    case CollectionChangedAction.Remove:
                        callback.OnRemoved(OldIndex, 1);
                        break;
                    case CollectionChangedAction.Replace:
                        callback.OnChanged(OldIndex, OldIndex, 1, false);
                        break;
                    case CollectionChangedAction.Changed:
                        callback.OnChanged(OldIndex, OldIndex, 1, false);
                        break;
                    default:
                        ExceptionManager.ThrowEnumOutOfRange(nameof(Action), Action);
                        break;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CollectionChangedEvent Changed(object? item, int index, object? args) => new(CollectionChangedAction.Changed, item, args, index, index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CollectionChangedEvent Add(object? item, int index) => new(CollectionChangedAction.Add, item, item, index, index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CollectionChangedEvent Replace(object? oldItem, object? newItem, int index) => new(CollectionChangedAction.Replace, oldItem, newItem, index, index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CollectionChangedEvent Move(object? item, int oldIndex, int newIndex) => new(CollectionChangedAction.Move, item, item, oldIndex, newIndex);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CollectionChangedEvent Remove(object? item, int index) => new(CollectionChangedAction.Remove, item, item, index, index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CollectionChangedEvent Reset(IEnumerable<object?>? items) =>
                new(CollectionChangedAction.Reset, null, items, -1, -1);
        }

        protected class WeakListener : ICollectionBatchUpdateListener, ICollectionDecoratorListener, IHasTarget<BindableCollectionAdapter?>, IHasPriority
        {
            internal readonly IWeakReference Reference;
            private readonly int _version;

            public WeakListener(BindableCollectionAdapter adapter, int version, WeakListener? oldListener)
            {
                Reference = oldListener?.Reference ?? adapter.ToWeakReference();
                _version = version;
            }

            public int Priority { get; set; } = CollectionComponentPriority.BindableAdapter;

            public BindableCollectionAdapter? Target => GetAdapter(null);

            public void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
            {
                if (sender == null)
                    return;

                var adapter = GetAdapter(sender);
                if (adapter == null || !adapter.IsAlive)
                    ((INotifyCollectionChanged)sender).CollectionChanged -= OnCollectionChanged;
                else
                    adapter.OnCollectionChanged(sender, args, _version);
            }

            public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType) => GetAdapter(collection)?.BeginBatchUpdate(_version);

            public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType) => GetAdapter(collection)?.EndBatchUpdate(_version);

            public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
            {
                var adapter = GetAdapter(collection);
                if (adapter != null && adapter.IsChangeEventSupported(item, args))
                    adapter.AddEvent(CollectionChangedEvent.Changed(item, index, args), _version);
            }

            public void OnAdded(IReadOnlyObservableCollection collection, object? item, int index) =>
                GetAdapter(collection)?.AddEvent(CollectionChangedEvent.Add(item, index), _version);

            public void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index) =>
                GetAdapter(collection)?.AddEvent(CollectionChangedEvent.Replace(oldItem, newItem, index), _version);

            public void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex) =>
                GetAdapter(collection)?.AddEvent(CollectionChangedEvent.Move(item, oldIndex, newIndex), _version);

            public void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index) =>
                GetAdapter(collection)?.AddEvent(CollectionChangedEvent.Remove(item, index), _version);

            public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items) =>
                GetAdapter(collection)?.AddEvent(CollectionChangedEvent.Reset(items), _version);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected BindableCollectionAdapter? GetAdapter(object? owner)
            {
                var adapter = (BindableCollectionAdapter?)Reference.Target;
                if (adapter != null && adapter.IsAlive)
                    return adapter;

                (owner as IComponentOwner)?.Components.Remove(this);
                return null;
            }
        }
    }
}