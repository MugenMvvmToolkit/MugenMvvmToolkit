using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public abstract class CollectionObserverBase : AttachableComponentBase<IReadOnlyObservableCollection>, ISuspendableComponent<IReadOnlyObservableCollection>,
        IDisposableComponent<IReadOnlyObservableCollection>, ISupportChangeNotification, IHasPriority
    {
        private ListInternal<IObserver?> _observers;
        private Dictionary<object, int>? _items;
        private PropertyChangedEventHandler? _handler;
        private volatile int _suspendCount;
        private bool _isNotificationsDirty;
#if !NET5_0
        private List<object?>? _oldItems;
#endif

        protected CollectionObserverBase()
        {
            _observers = new ListInternal<IObserver?>(2);
        }

        public int Priority { get; init; }

        public ActionToken AddCollectionObserver<TState>(TState state, Action<TState, IReadOnlyObservableCollection> onChanged, int delay = 0) =>
            AddCollectionObserverInternal(onChanged, state, delay, false);

        public ActionToken AddCollectionObserverWeak<TTarget>(TTarget target, Action<TTarget, IReadOnlyObservableCollection> onChanged, int delay = 0) where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            return AddCollectionObserverInternal(onChanged, target, delay, true);
        }

        public ActionToken AddItemObserver<T>(Func<ChangedEventInfo<T>, bool> predicate, Action<ChangedEventInfo<T>> onChanged, int delay = 0)
            where T : class => AddItemObserverInternal<T, (Func<ChangedEventInfo<T>, bool>, Action<ChangedEventInfo<T>>)>((s, info) => s.Item1(info), (s, info) => s.Item2(info),
            (predicate, onChanged), delay, false);

        public ActionToken AddItemObserver<T, TState>(Func<TState, ChangedEventInfo<T>, bool> predicate, Action<TState, ChangedEventInfo<T>> onChanged, TState state,
            int delay = 0) where T : class =>
            AddItemObserverInternal(predicate, onChanged, state, delay, false);

        public ActionToken AddItemObserverWeak<T, TTarget>(TTarget target, Func<TTarget, ChangedEventInfo<T>, bool> predicate,
            Action<TTarget, ChangedEventInfo<T>> onChanged, int delay = 0)
            where T : class
            where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            return AddItemObserverInternal(predicate, onChanged, target, delay, true);
        }

        public void ClearObservers()
        {
            lock (this)
            {
                var observers = _observers.Items;
                for (var i = 0; i < _observers.Count; i++)
                    observers[i]?.Dispose();
                _observers.Clear();
            }
        }

        public void RaiseChanged(bool force = false, IReadOnlyMetadataContext? metadata = null) => OnChanged(null, null, force);

        protected abstract IEnumerable<object?>? GetItems();

        protected virtual void OnCollectionChanged() => OnChanged(null, null);

        protected virtual void OnChanged(object? item, string? member, bool force = false)
        {
            if (_suspendCount != 0)
            {
                _isNotificationsDirty = true;
                return;
            }

            var hasDeadRefs = false;
            // ReSharper disable InconsistentlySynchronizedField
            var items = _observers.Items;
            var count = Math.Min(_observers.Count, items.Length);
            // ReSharper restore InconsistentlySynchronizedField
            for (var i = 0; i < count; i++)
            {
                var observer = items[i];
                if (observer == null)
                {
                    hasDeadRefs = true;
                    continue;
                }

                if (!observer.OnChanged(item, member, force))
                {
                    hasDeadRefs = true;
                    items[i] = null;
                }
            }

            if (!hasDeadRefs)
                return;
            lock (this)
            {
                for (var i = 0; i < _observers.Count; i++)
                {
                    if (_observers.Items[i] == null)
                    {
                        _observers.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        protected virtual bool TrySubscribe(object item)
        {
            if (item is INotifyPropertyChanged propertyChanged)
            {
                propertyChanged.PropertyChanged += _handler ??= OnPropertyChanged;
                return true;
            }

            return false;
        }

        protected virtual void Unsubscribe(object item) => ((INotifyPropertyChanged) item).PropertyChanged -= _handler;

        protected override void OnAttached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            Subscribe(false);
            OnCollectionChanged();
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            using (owner.TryLock())
            {
                Clear();
            }
        }

        protected void OnAdded(object? item)
        {
            SubscribeIfNeed(item);
            OnCollectionChanged();
        }

        protected void OnReplaced(object? oldItem, object? newItem)
        {
            UnsubscribeIfNeed(oldItem);
            SubscribeIfNeed(newItem);
            OnCollectionChanged();
        }

        protected void OnMoved(object? item) => OnCollectionChanged();

        protected void OnRemoved(object? item)
        {
            UnsubscribeIfNeed(item);
            OnCollectionChanged();
        }

        protected void OnReset(IEnumerable<object?>? oldItems, IEnumerable<object?>? items)
        {
            if (_items == null)
            {
                OnCollectionChanged();
                return;
            }

            if (items == null)
            {
                foreach (var item in _items)
                    Unsubscribe(item.Key);
                _items.Clear();
                OnCollectionChanged();
                return;
            }

            if (oldItems == null)
            {
#if NET5_0
                foreach (var item in _items)
                    _items[item.Key] = 0;
#else
                _oldItems ??= new List<object?>(_items.Count);
                _oldItems.Clear();
                foreach (var item in _items)
                {
                    for (var i = 0; i < item.Value; i++)
                        _oldItems.Add(item.Key);
                }

                oldItems = _oldItems;
#endif
            }

            foreach (var item in items)
                SubscribeIfNeed(item);

            if (oldItems == null)
            {
                foreach (var item in _items)
                {
                    if (item.Value == 0)
                    {
                        _items.Remove(item.Key);
                        Unsubscribe(item.Key);
                    }
                }
            }
            else
            {
                foreach (var item in oldItems)
                    UnsubscribeIfNeed(item);
            }
#if !NET5_0
            _oldItems?.Clear();
#endif
            OnCollectionChanged();
        }

        private ActionToken AddCollectionObserverInternal<TState>(Action<TState, IReadOnlyObservableCollection> onChanged, TState state, int delay, bool weak)
        {
            Should.NotBeNull(onChanged, nameof(onChanged));
            IObserver observer;
            if (weak)
                observer = new Observer<object, TState>(this, (_, info) => info.IsCollectionEvent, onChanged.AsChangedDelegate, state, delay, weak);
            else
            {
                observer = new Observer<object, (Action<TState, IReadOnlyObservableCollection>, TState)>(this, (_, info) => info.IsCollectionEvent,
                    (s, info) => s.Item1(s.Item2, info.Collection), (onChanged, state), delay, false);
            }

            lock (this)
            {
                _observers.Add(observer);
            }

            if (IsAttached)
                observer.OnChanged(null, null, false);
            return ActionToken.FromDelegate((l, item) => ((CollectionObserverBase) l!).RemoveObserver((IObserver) item!), this, observer);
        }

        private ActionToken AddItemObserverInternal<T, TState>(Func<TState, ChangedEventInfo<T>, bool> predicate, Action<TState, ChangedEventInfo<T>> onChanged, TState state,
            int delay, bool weak)
            where T : class
        {
            Should.NotBeNull(onChanged, nameof(onChanged));
            Should.NotBeNull(predicate, nameof(predicate));
            var observer = new Observer<T, TState>(this, predicate, onChanged, state, delay, weak);
            lock (this)
            {
                _observers.Add(observer);
            }

            Subscribe(true);
            if (IsAttached)
                observer.OnChanged(null, null, false);
            return ActionToken.FromDelegate((l, item) => ((CollectionObserverBase) l!).RemoveObserver((IObserver) item!), this, observer);
        }

        private void RemoveObserver(IObserver observer)
        {
            bool removed;
            lock (this)
            {
                removed = _observers.Remove(observer);
            }

            if (removed)
                observer.Dispose();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => OnChanged(sender, args.PropertyName ?? "");

        private void SubscribeIfNeed(object? item)
        {
            if (item == null || _items == null)
                return;

            if (_items.TryGetValue(item, out var count) || TrySubscribe(item))
                _items[item] = count + 1;
        }

        private void UnsubscribeIfNeed(object? item)
        {
            if (item == null || _items == null || !_items.TryGetValue(item, out var count))
                return;
            if (--count == 0)
            {
                Unsubscribe(item);
                _items.Remove(item);
            }
            else
                _items[item] = count - 1;
        }

        private void Subscribe(bool force)
        {
            if (force)
            {
                if (_items == null)
                    Interlocked.Exchange(ref _items, new Dictionary<object, int>(7, InternalEqualityComparer.Reference));
            }

            if (_items == null)
                return;

            var items = GetItems();
            if (items != null)
            {
                foreach (var item in items)
                    SubscribeIfNeed(item);
            }
        }

        private void Clear()
        {
            if (_items == null)
                return;
            foreach (var item in _items)
                Unsubscribe(item.Key);
            _items.Clear();
        }

        private void EndSuspend()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
            {
                _isNotificationsDirty = false;
                OnChanged(null, null);
            }
        }

        void IDisposableComponent<IReadOnlyObservableCollection>.Dispose(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            ClearObservers();
        }

        bool ISuspendableComponent<IReadOnlyObservableCollection>.IsSuspended(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => _suspendCount != 0;

        ActionToken ISuspendableComponent<IReadOnlyObservableCollection>.TrySuspend(IReadOnlyObservableCollection owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate(this, t => t.EndSuspend());
        }

        private interface IObserver : IDisposable
        {
            public bool OnChanged(object? item, string? member, bool force);
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct ChangedEventInfo<T> : IEquatable<ChangedEventInfo<T>> where T : class
        {
            public readonly IReadOnlyObservableCollection Collection;
            public readonly T? Item;
            public readonly string? Member;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ChangedEventInfo(IReadOnlyObservableCollection collection, T? item, string? member)
            {
                Collection = collection;
                Item = item;
                Member = member;
            }

            public bool IsCollectionEvent
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Item == null && Member == null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsMemberChanged(string member, bool emptyMemberResult = true) => Item != null && (member == Member || Member == "" && emptyMemberResult);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsMemberOrCollectionChanged(string member, bool emptyMemberResult = true) => Item == null || member == Member || Member == "" && emptyMemberResult;

            public bool Equals(ChangedEventInfo<T> other) =>
                ReferenceEquals(Collection, other.Collection) && EqualityComparer<T?>.Default.Equals(Item, other.Item) && Member == other.Member;

            public override bool Equals(object? obj) => obj is ChangedEventInfo<T> other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Collection, Item, Member);
        }

        private sealed class Observer<T, TTargetOrState> : IObserver where T : class
        {
            private readonly CollectionObserverBase _observer;
            private readonly IWeakReference? _targetRef;
            private readonly TTargetOrState? _state;
            private readonly Action<TTargetOrState, ChangedEventInfo<T>> _onChanged;
            private readonly Func<TTargetOrState, ChangedEventInfo<T>, bool> _predicate;
            private readonly int _delay;
            private Timer? _timer;
            private HashSet<ChangedEventInfo<T>>? _pendingItems;
            private ListInternal<ChangedEventInfo<T>> _notifications;

            public Observer(CollectionObserverBase observer, Func<TTargetOrState, ChangedEventInfo<T>, bool> predicate,
                Action<TTargetOrState, ChangedEventInfo<T>> onChanged, TTargetOrState state, int delay, bool weak)
            {
                _observer = observer;
                if (weak)
                    _targetRef = state.ToWeakReference();
                else
                    _state = state;

                _onChanged = onChanged;
                _predicate = predicate;
                _delay = delay;
                if (delay != 0)
                    _timer = WeakTimer.Get(this, o => o.Raise());
            }

            public void Dispose()
            {
                _timer?.Dispose();
                _timer = null;
            }

            public bool OnChanged(object? item, string? member, bool force)
            {
                var collection = _observer.OwnerOptional;
                if (collection == null)
                    return true;

                if (item is not T itemT)
                {
                    if (item != null)
                        return true;

                    itemT = null!;
                }

                var eventInfo = new ChangedEventInfo<T>(collection, itemT, member);
                if (!CanInvoke(eventInfo, out var r))
                    return r;

                if (_delay == 0 || force)
                    return OnChanged(eventInfo);

                lock (this)
                {
                    _pendingItems ??= new HashSet<ChangedEventInfo<T>>();
                    _pendingItems.Add(eventInfo);
                }

                _timer?.Change(_delay, Timeout.Infinite);
                return true;
            }

            private bool CanInvoke(ChangedEventInfo<T> eventInfo, out bool result)
            {
                if (_targetRef == null)
                {
                    result = true;
                    return _predicate(_state!, eventInfo);
                }

                var target = (TTargetOrState?) _targetRef.Target;
                if (target == null)
                {
                    result = false;
                    return false;
                }

                result = true;
                return _predicate(target, eventInfo);
            }

            private bool OnChanged(ChangedEventInfo<T> eventInfo)
            {
                if (_targetRef == null)
                {
                    _onChanged(_state!, eventInfo);
                    return true;
                }

                var target = (TTargetOrState?) _targetRef.Target;
                if (target == null)
                    return false;

                _onChanged(target, eventInfo);
                return true;
            }

            private void Raise()
            {
                lock (this)
                {
                    if (_notifications.IsEmpty)
                        _notifications = new ListInternal<ChangedEventInfo<T>>(_pendingItems!.Count);
                    foreach (var item in _pendingItems!)
                        _notifications.Add(item);
                    _pendingItems.Clear();
                }

                for (int i = 0; i < _notifications.Count; i++)
                    OnChanged(_notifications.Items[i]);
                _notifications.Clear();
            }
        }
    }
}