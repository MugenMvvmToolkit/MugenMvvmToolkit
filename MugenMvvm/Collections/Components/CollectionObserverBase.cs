using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Enums;
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

        public ActionToken AddObserver<T>(Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay,
            bool listenItemChanges)
            where T : class => AddObserverInternal<T, (Func<CollectionChangedEventInfo<T>, bool>, Action<ItemOrArray<CollectionChangedEventInfo<T>>>)>((s, info) => s.Item1(info),
            (s, info) => s.Item2(info),
            (predicate, onChanged), delay, false, listenItemChanges);

        public ActionToken AddObserver<T, TState>(Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            TState state, int delay, bool listenItemChanges) where T : class =>
            AddObserverInternal(predicate, onChanged, state, delay, false, listenItemChanges);

        public ActionToken AddObserverWeak<T, TTarget>(TTarget target, Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate,
            Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay, bool listenItemChanges)
            where T : class
            where TTarget : class
        {
            Should.NotBeNull(target, nameof(target));
            return AddObserverInternal(predicate, onChanged, target, delay, true, listenItemChanges);
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

        public void RaiseChanged(bool force = false, IReadOnlyMetadataContext? metadata = null)
        {
            var owner = OwnerOptional;
            if (owner != null)
                OnChanged(owner, CollectionChangedAction.Reset, null, GetItems(), force);
        }

        protected abstract IEnumerable<object?>? GetItems();

        protected virtual void OnCollectionChanged(IReadOnlyObservableCollection collection, CollectionChangedAction action, object? item, object? parameter) =>
            OnChanged(collection, action, item, parameter);

        protected virtual void OnChanged(IReadOnlyObservableCollection collection, CollectionChangedAction action, object? item, object? parameter, bool force = false)
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

                if (!observer.OnChanged(collection, action, item, parameter, force))
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

        protected virtual bool IsObservable(object item)
        {
            // ReSharper disable InconsistentlySynchronizedField
            var items = _observers.Items;
            var count = Math.Min(_observers.Count, items.Length);
            // ReSharper restore InconsistentlySynchronizedField
            for (var i = 0; i < count; i++)
            {
                var observer = items[i];
                if (observer != null && observer.IsSupported(item))
                    return true;
            }

            return false;
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

        protected virtual void Unsubscribe(object item) => ((INotifyPropertyChanged)item).PropertyChanged -= _handler;

        protected override void OnAttached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            Resubscribe(owner);
            OnCollectionChanged(owner, CollectionChangedAction.Reset, null, GetItems());
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected void OnAdded(IReadOnlyObservableCollection collection, object? item)
        {
            SubscribeIfNeed(item);
            OnCollectionChanged(collection, CollectionChangedAction.Add, item, null);
        }

        protected void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem)
        {
            UnsubscribeIfNeed(oldItem);
            SubscribeIfNeed(newItem);
            OnCollectionChanged(collection, CollectionChangedAction.Replace, newItem, oldItem);
        }

        protected void OnMoved(IReadOnlyObservableCollection collection, object? item) => OnCollectionChanged(collection, CollectionChangedAction.Move, item, null);

        protected void OnRemoved(IReadOnlyObservableCollection collection, object? item)
        {
            UnsubscribeIfNeed(item);
            OnCollectionChanged(collection, CollectionChangedAction.Remove, item, null);
        }

        protected void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? oldItems, IEnumerable<object?>? items)
        {
            if (_items == null)
            {
                OnCollectionChanged(collection, CollectionChangedAction.Reset, null, items);
                return;
            }

            if (items == null)
            {
                foreach (var item in _items)
                    Unsubscribe(item.Key);
                _items.Clear();
                OnCollectionChanged(collection, CollectionChangedAction.Reset, null, null);
                return;
            }

            Resubscribe(oldItems, items);
            OnCollectionChanged(collection, CollectionChangedAction.Reset, null, items);
        }

        protected ActionToken AddObserver(IObserver observer)
        {
            lock (this)
            {
                _observers.Add(observer);
            }

            return ActionToken.FromDelegate((l, item) => ((CollectionObserverBase)l!).RemoveObserver((IObserver)item!), this, observer);
        }

        private ActionToken AddObserverInternal<T, TState>(Func<TState, CollectionChangedEventInfo<T>, bool> predicate,
            Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, TState state, int delay, bool weak, bool listenItemChanges)
            where T : class
        {
            Should.NotBeNull(onChanged, nameof(onChanged));
            Should.NotBeNull(predicate, nameof(predicate));
            var token = AddObserver(new Observer<T, TState>(predicate, onChanged, state, delay, weak, listenItemChanges));
            if (listenItemChanges)
                Resubscribe(OwnerOptional);
            return token;
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

        private void Resubscribe(IReadOnlyObservableCollection? collection)
        {
            if (collection == null)
                return;
            using (collection.Lock())
            {
                var items = GetItems();
                if (items != null)
                    Resubscribe(null, items);
            }
        }

        private void Resubscribe(IEnumerable<object?>? oldItems, IEnumerable<object?> items)
        {
            var hasSubs = _items != null;
            if (hasSubs && oldItems == null)
            {
#if NET5_0
                foreach (var item in _items!)
                    _items[item.Key] = 0;
#else
                _oldItems ??= new List<object?>(_items!.Count);
                _oldItems.Clear();
                foreach (var item in _items!)
                {
                    for (var i = 0; i < item.Value; i++)
                        _oldItems.Add(item.Key);
                }

                oldItems = _oldItems;
#endif
            }

            foreach (var item in items)
                SubscribeIfNeed(item);

            if (hasSubs)
            {
                if (oldItems == null)
                {
                    foreach (var item in _items!)
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
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            var owner = OwnerOptional;
            if (owner != null && sender != null)
                OnChanged(owner, CollectionChangedAction.Changed, sender, args.PropertyName ?? "");
        }

        private void SubscribeIfNeed(object? item)
        {
            if (item == null || !IsObservable(item))
                return;

            _items ??= new Dictionary<object, int>(7, InternalEqualityComparer.Reference);
            if (_items.TryGetValue(item, out var count) || TrySubscribe(item))
                _items[item] = count + 1;
        }

        private void UnsubscribeIfNeed(object? item)
        {
            if (item == null || _items == null || !_items.TryGetValue(item, out var count))
                return;
            if (count == 1)
            {
                Unsubscribe(item);
                _items.Remove(item);
            }
            else
                _items[item] = count - 1;
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
                RaiseChanged();
            }
        }

        void IDisposableComponent<IReadOnlyObservableCollection>.Dispose(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            ClearObservers();
        }

        bool ISuspendableComponent<IReadOnlyObservableCollection>.IsSuspended(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => _suspendCount != 0;

        ActionToken ISuspendableComponent<IReadOnlyObservableCollection>.TrySuspend(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate(this, t => t.EndSuspend());
        }

        protected interface IObserver : IDisposable
        {
            bool IsSupported(object item);

            bool OnChanged(IReadOnlyObservableCollection collection, CollectionChangedAction action, object? item, object? parameter, bool force);
        }

        private sealed class Observer<T, TTargetOrState> : IObserver where T : class
        {
            private readonly IWeakReference? _targetRef;
            private readonly TTargetOrState? _state;
            private readonly Action<TTargetOrState, ItemOrArray<CollectionChangedEventInfo<T>>> _onChanged;
            private readonly Func<TTargetOrState, CollectionChangedEventInfo<T>, bool> _predicate;
            private readonly int _delay;
            private readonly bool _listenItemChanges;
            private Timer? _timer;
            private HashSet<CollectionChangedEventInfo<T>>? _pendingItems;

            public Observer(Func<TTargetOrState, CollectionChangedEventInfo<T>, bool> predicate, Action<TTargetOrState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
                TTargetOrState state, int delay, bool weak, bool listenItemChanges)
            {
                if (weak)
                    _targetRef = state.ToWeakReference();
                else
                    _state = state;

                _onChanged = onChanged;
                _predicate = predicate;
                _delay = delay;
                _listenItemChanges = listenItemChanges;
                if (delay != 0)
                    _timer = WeakTimer.Get(this, o => o.TimerCallback());
            }

            public void Dispose()
            {
                _timer?.Dispose();
                _timer = null;
            }

            public bool IsSupported(object item) => _listenItemChanges && item is T and INotifyPropertyChanged;

            public bool OnChanged(IReadOnlyObservableCollection collection, CollectionChangedAction action, object? item, object? parameter, bool force)
            {
                if (item is not T itemT)
                {
                    if (item != null)
                        return true;

                    itemT = null!;
                }

                var eventInfo = new CollectionChangedEventInfo<T>(collection, itemT, parameter, action);
                if (!CanInvoke(eventInfo, out var r))
                    return r;

                if (_delay == 0)
                    return OnChanged(eventInfo);

                lock (this)
                {
                    _pendingItems ??= new HashSet<CollectionChangedEventInfo<T>>();
                    if (eventInfo.Action == CollectionChangedAction.Reset)
                        _pendingItems.Clear();
                    _pendingItems.Add(eventInfo);
                }

                if (force)
                {
                    _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                    TimerCallback();
                }
                else
                    _timer?.Change(_delay, Timeout.Infinite);

                return true;
            }

            private bool CanInvoke(CollectionChangedEventInfo<T> eventInfo, out bool result)
            {
                if (_targetRef == null)
                {
                    result = true;
                    return _predicate(_state!, eventInfo);
                }

                var target = (TTargetOrState?)_targetRef.Target;
                if (target == null)
                {
                    result = false;
                    return false;
                }

                result = true;
                return _predicate(target, eventInfo);
            }

            private bool OnChanged(ItemOrArray<CollectionChangedEventInfo<T>> eventInfo)
            {
                if (_targetRef == null)
                {
                    _onChanged(_state!, eventInfo);
                    return true;
                }

                var target = (TTargetOrState?)_targetRef.Target;
                if (target == null)
                    return false;

                _onChanged(target, eventInfo);
                return true;
            }

            private void TimerCallback()
            {
                ItemOrArray<CollectionChangedEventInfo<T>> notifications;
                lock (this)
                {
                    notifications = ItemOrArray.Get<CollectionChangedEventInfo<T>>(_pendingItems!.Count);
                    var index = 0;
                    foreach (var item in _pendingItems!)
                        notifications.SetAt(index++, item);
                    _pendingItems.Clear();
                }

                if (notifications.Count != 0)
                    OnChanged(notifications);
            }
        }
    }
}