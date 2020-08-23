﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    public class BindableCollectionAdapter : IReadOnlyList<object?>, INotifyCollectionChanged, IThreadDispatcherHandler, IValueHolder<Delegate>
    {
        #region Fields

        private readonly List<CollectionChangedEvent> _pendingEvents;
        private readonly IThreadDispatcher? _threadDispatcher;

        protected readonly List<object?> Items;
        private IEnumerable? _collection;

        private List<CollectionChangedEvent>? _eventsCache;
        private ThreadExecutionMode _executionMode;
        private List<object?>? _resetCache;
        private int _suspendCount;
        protected WeakListener? Listener;

        #endregion

        #region Constructors

        public BindableCollectionAdapter(IThreadDispatcher? threadDispatcher = null)
        {
            _threadDispatcher = threadDispatcher;
            _pendingEvents = new List<CollectionChangedEvent>();
            _executionMode = ThreadExecutionMode.Main;
            Items = new List<object?>();
        }

        #endregion

        #region Properties

        public int Count => Items.Count;

        public object? this[int index] => Items[index];

        public IEnumerable? Collection
        {
            get => _collection;
            set
            {
                if (ReferenceEquals(value, _collection))
                    return;

                int version;
                lock (_pendingEvents)
                {
                    version = ++Version;
                    if (_collection != null)
                        RemoveCollectionListener(_collection);
                    _suspendCount = 0;
                    _pendingEvents.Clear();
                    _collection = value;
                    if (value != null)
                    {
                        AddEvent(CollectionChangedEvent.Reset(GetCollectionItems(value)), version);
                        AddCollectionListener(value, version);
                        return;
                    }
                }

                AddEvent(CollectionChangedEvent.Clear(), version);
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

        public bool IgnoreItemChangedEvent { get; set; } = true;

        public int EventsResetLimit { get; set; } = 50;

        protected int Version { get; private set; } = -1;

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected virtual bool IsAlive => true;

        protected bool HasCollectionChangedListeners => CollectionChanged != null;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        #endregion

        #region Implementation of interfaces

        public IEnumerator<object?> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IThreadDispatcherHandler.Execute(object? state) => ExecutePendingEvents((int) state!);

        #endregion

        #region Methods

        public int IndexOf(object? item) => Items.IndexOf(item);

        protected virtual void AddCollectionListener(IEnumerable collection, int version)
        {
            if (collection is IObservableCollection components)
            {
                Listener = Listener ??= new WeakListener(this);
                Listener.SetVersion(version);
                components.Components.Add(Listener);
            }
            else if (collection is INotifyCollectionChanged notifyCollectionChanged)
            {
                Listener = Listener ??= new WeakListener(this);
                Listener.SetVersion(version);
                notifyCollectionChanged.CollectionChanged += Listener.OnCollectionChanged;
            }
        }

        protected virtual void RemoveCollectionListener(IEnumerable collection)
        {
            if (Listener == null)
                return;
            if (collection is IObservableCollection hasComponents)
                hasComponents.Components.Remove(Listener);
            else if (collection is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged -= Listener.OnCollectionChanged;
            Listener.SetVersion(int.MinValue);
        }

        protected virtual IEnumerable<object?> GetCollectionItems(IEnumerable collection)
        {
            if (collection is IObservableCollectionBase observable)
                return observable.DecorateItems();
            return collection as IEnumerable<object?> ?? collection.Cast<object>();
        }

        protected virtual void OnItemChanged(object? item, int index, object? args, bool batchUpdate, int version)
        {
        }

        protected virtual void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            Items.Insert(index, item);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected virtual void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            Items[index] = newItem;
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected virtual void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, item);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        protected virtual void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            Items.RemoveAt(index);
            if (HasCollectionChangedListeners)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected virtual void OnReset(IEnumerable<object?> items, bool batchUpdate, int version)
        {
            Items.Clear();
            Items.AddRange(items);
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
        }

        protected virtual void OnCleared(bool batchUpdate, int version)
        {
            Items.Clear();
            RaiseCollectionChanged(Default.ResetCollectionEventArgs);
        }

        protected virtual void BatchUpdate(List<CollectionChangedEvent> events, int version)
        {
            if (events.Count < EventsResetLimit)
            {
                for (var i = 0; i < events.Count; i++)
                    events[i].Raise(this, true, version);
                return;
            }

            _resetCache ??= new List<object?>();
            _resetCache.Clear();
            _resetCache.AddRange(Items);

            for (var i = 0; i < events.Count; i++)
                events[i].ApplyToSource(_resetCache);
            OnReset(_resetCache, true, version);
            _resetCache.Clear();
        }

        protected virtual bool AddPendingEvent(List<CollectionChangedEvent> pendingEvents, in CollectionChangedEvent e)
        {
            if (e.Action == CollectionChangedAction.Reset || e.Action == CollectionChangedAction.Clear)
            {
                pendingEvents.Clear();
                if (e.Action == CollectionChangedAction.Reset)
                {
                    pendingEvents.Add(CollectionChangedEvent.Reset(((IEnumerable<object?>) e.NewItem!).ToList()));
                    return true;
                }
            }

            pendingEvents.Add(e);
            return true;
        }

        protected virtual List<CollectionChangedEvent> GetPendingEvents(List<CollectionChangedEvent> pendingEvents)
        {
            _eventsCache ??= new List<CollectionChangedEvent>();
            _eventsCache.Clear();
            _eventsCache.AddRange(pendingEvents);
            return _eventsCache;
        }

        protected bool AddEvent(in CollectionChangedEvent collectionChangedEvent, int version)
        {
            if (version != Version || IgnoreItemChangedEvent && collectionChangedEvent.Action == CollectionChangedAction.Changed)
                return false;

            var canExecuteInline = ThreadDispatcher.CanExecuteInline(ExecutionMode);
            var inline = canExecuteInline;
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
            {
                if (canExecuteInline)
                    ExecutePendingEvents(version);
                else
                    EndBatchUpdate(version);
            }

            return true;
        }

        protected void BeginBatchUpdate(int version)
        {
            if (Version != version)
                return;
            lock (_pendingEvents)
            {
                if (version == Version)
                    ++_suspendCount;
            }
        }

        protected void EndBatchUpdate(int version) => ThreadDispatcher.Execute(ExecutionMode, this, BoxingExtensions.Box(version));

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
                    for (var i = 0; i < e.NewItems.Count; i++)
                        AddEvent(CollectionChangedEvent.Add(e.NewItems[i], e.NewStartingIndex + i), version);
                    break;
                case NotifyCollectionChangedAction.Move:
                    AddEvent(CollectionChangedEvent.Move(e.OldItems[0], e.OldStartingIndex, e.NewStartingIndex), version);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        AddEvent(CollectionChangedEvent.Remove(e.OldItems[i], e.OldStartingIndex + i), version);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    AddEvent(CollectionChangedEvent.Replace(e.OldItems[0], e.NewItems[0], e.NewStartingIndex), version);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    AddEvent(CollectionChangedEvent.Clear(), version);
                    break;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    break;
            }
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);

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
                    if (e.OldItems.Count == 1)
                        return false;
                    items = GetCollectionItems((IEnumerable) sender);
                    return true;
                case NotifyCollectionChangedAction.Reset:
                    items = GetCollectionItems((IEnumerable) sender);
                    return items.Count() != 0;
                default:
                    ExceptionManager.ThrowEnumOutOfRange(nameof(e.Action), e.Action);
                    return false;
            }
        }

        private void ExecutePendingEvents(int version)
        {
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

        #endregion

        #region Nested types

        protected class WeakListener : AttachableComponentBase<IObservableCollection>, ICollectionBatchUpdateListener, ICollectionDecoratorListener, IHasPriority
        {
            #region Fields

            private readonly IWeakReference _reference;
            private int _version;

            #endregion

            #region Constructors

            public WeakListener(BindableCollectionAdapter adapter)
            {
                _reference = adapter.ToWeakReference();
            }

            #endregion

            #region Properties

            public int Priority { get; set; } = ComponentPriority.PostInitializer;

            #endregion

            #region Implementation of interfaces

            public void OnBeginBatchUpdate(IObservableCollection collection) => GetAdapter()?.BeginBatchUpdate(_version);

            public void OnEndBatchUpdate(IObservableCollection collection) => GetAdapter()?.EndBatchUpdate(_version);

            public void OnItemChanged(IObservableCollection collection, object? item, int index, object? args) => GetAdapter()?.AddEvent(CollectionChangedEvent.Changed(item, index, args), _version);

            public void OnAdded(IObservableCollection collection, object? item, int index) => GetAdapter()?.AddEvent(CollectionChangedEvent.Add(item, index), _version);

            public void OnReplaced(IObservableCollection collection, object? oldItem, object? newItem, int index) => GetAdapter()?.AddEvent(CollectionChangedEvent.Replace(oldItem, newItem, index), _version);

            public void OnMoved(IObservableCollection collection, object? item, int oldIndex, int newIndex) => GetAdapter()?.AddEvent(CollectionChangedEvent.Move(item, oldIndex, newIndex), _version);

            public void OnRemoved(IObservableCollection collection, object? item, int index) => GetAdapter()?.AddEvent(CollectionChangedEvent.Remove(item, index), _version);

            public void OnReset(IObservableCollection collection, IEnumerable<object?> items) => GetAdapter()?.AddEvent(CollectionChangedEvent.Reset(items), _version);

            public void OnCleared(IObservableCollection collection) => GetAdapter()?.AddEvent(CollectionChangedEvent.Clear(), _version);

            #endregion

            #region Methods

            public void SetVersion(int version) => _version = version;

            public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            {
                var adapter = GetAdapter();
                if (adapter == null || !adapter.IsAlive)
                    ((INotifyCollectionChanged) sender).CollectionChanged -= OnCollectionChanged;
                else
                    adapter.OnCollectionChanged(sender, args, _version);
            }

            protected override void OnAttached(IObservableCollection owner, IReadOnlyMetadataContext? metadata) => owner.GetOrAddCollectionDecoratorManager();

            protected BindableCollectionAdapter? GetAdapter()
            {
                var adapter = (BindableCollectionAdapter?) _reference.Target;
                if (adapter == null || !adapter.IsAlive)
                {
                    OwnerOptional?.Components.Remove(this);
                    return null;
                }

                return adapter;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        protected readonly struct CollectionChangedEvent
        {
            #region Fields

            public readonly CollectionChangedAction Action;
            public readonly int NewIndex;
            public readonly object? NewItem;
            public readonly int OldIndex;
            public readonly object? OldItem;

            #endregion

            #region Constructors

            public CollectionChangedEvent(CollectionChangedAction action, object? oldItem, object? newItem, int oldIndex, int newIndex)
            {
                Action = action;
                OldItem = oldItem;
                NewItem = newItem;
                OldIndex = oldIndex;
                NewIndex = newIndex;
            }

            #endregion

            #region Properties

            public bool IsEmpty => Action == 0;

            public IEnumerable<object?>? ResetItems => (IEnumerable<object?>?) NewItem;

            public object? ChangedArgs => NewItem;

            #endregion

            #region Implementation of interfaces

            public bool Raise(BindableCollectionAdapter adapter, bool batchUpdate, int version)
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
                    case CollectionChangedAction.Clear:
                        adapter.OnCleared(batchUpdate, version);
                        break;
                    case CollectionChangedAction.Reset:
                        adapter.OnReset((IEnumerable<object?>) NewItem!, batchUpdate, version);
                        break;
                    case CollectionChangedAction.Changed:
                        adapter.OnItemChanged(OldItem, OldIndex, NewItem, batchUpdate, version);
                        break;
                    default:
                        ExceptionManager.ThrowEnumOutOfRange(nameof(Action), Action);
                        break;
                }

                return true;
            }

            public bool ApplyToSource(IList<object?> source)
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
                    case CollectionChangedAction.Clear:
                        source.Clear();
                        break;
                    case CollectionChangedAction.Reset:
                        source.Clear();
                        source.AddRange((IEnumerable<object?>) NewItem!);
                        break;
                    default:
                        return false;
                }

                return true;
            }

            #endregion

            #region Methods

            public static CollectionChangedEvent Changed(object? item, int index, object? args) => new CollectionChangedEvent(CollectionChangedAction.Changed, item, args, index, index);

            public static CollectionChangedEvent Add(object? item, int index) => new CollectionChangedEvent(CollectionChangedAction.Add, item, item, index, index);

            public static CollectionChangedEvent Replace(object? oldItem, object? newItem, int index) => new CollectionChangedEvent(CollectionChangedAction.Replace, oldItem, newItem, index, index);

            public static CollectionChangedEvent Move(object? item, int oldIndex, int newIndex) => new CollectionChangedEvent(CollectionChangedAction.Move, item, item, oldIndex, newIndex);

            public static CollectionChangedEvent Remove(object? item, int index) => new CollectionChangedEvent(CollectionChangedAction.Remove, item, item, index, index);

            public static CollectionChangedEvent Reset(IEnumerable<object?> items) => new CollectionChangedEvent(CollectionChangedAction.Reset, null, items, -1, -1);

            public static CollectionChangedEvent Clear() => new CollectionChangedEvent(CollectionChangedAction.Clear, default!, default!, -1, -1);

            #endregion
        }

        protected enum CollectionChangedAction
        {
            Add = 1,
            Move = 2,
            Remove = 3,
            Replace = 4,
            Clear = 5,
            Reset = 6,
            Changed = 7
        }

        #endregion
    }
}