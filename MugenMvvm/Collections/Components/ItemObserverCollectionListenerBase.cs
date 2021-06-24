using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public abstract class ItemObserverCollectionListenerBase<T> : ISuspendableComponent<IReadOnlyObservableCollection>, IDisposableComponent<IReadOnlyObservableCollection>, IAttachableComponent, IDetachableComponent,
        IHasPriority where T : class?
    {
        private readonly Dictionary<T, int> _items;
        private readonly PropertyChangedEventHandler _handler;
        private readonly ListInternal<Observer> _observers;
        private volatile int _suspendCount;
        private bool _isNotificationsDirty;
#if !NET5_0
        private List<T>? _oldItems;
#endif

        protected ItemObserverCollectionListenerBase(IEqualityComparer<T>? comparer)
        {
            _handler = OnPropertyChanged;
            _observers = new ListInternal<Observer>(2);
            _items = new Dictionary<T, int>(comparer ?? EqualityComparer<T>.Default);
        }

        public int Priority { get; set; }

        public ActionToken AddObserver<TState>(TState state, Func<TState, ChangedEventInfo, bool> canInvoke, Action<TState, T?> invokeAction, int delay = 0)
        {
            Should.NotBeNull(invokeAction, nameof(invokeAction));
            Should.NotBeNull(canInvoke, nameof(canInvoke));
            var observer = new Observer<TState>(state, canInvoke, invokeAction, delay);
            lock (_observers)
            {
                _observers.Add(observer);
            }

            return ActionToken.FromDelegate((l, item) => ((ItemObserverCollectionListenerBase<T>)l!).RemoveObserver((Observer)item!), this, observer);
        }

        public void ClearObservers()
        {
            lock (_observers)
            {
                for (var i = 0; i < _observers.Count; i++)
                    _observers.Items[i].Dispose();
                _observers.Clear();
            }
        }

        protected virtual void OnChanged(T? item, string? member)
        {
            if (_suspendCount != 0)
            {
                _isNotificationsDirty = true;
                return;
            }

            // ReSharper disable InconsistentlySynchronizedField
            var items = _observers.Items;
            var count = Math.Min(_observers.Count, items.Length);
            // ReSharper restore InconsistentlySynchronizedField
            for (var i = 0; i < count; i++)
                items[i]?.OnChanged(item, member);
        }

        protected virtual bool TrySubscribe(T item)
        {
            if (item is INotifyPropertyChanged propertyChanged)
            {
                propertyChanged.PropertyChanged += _handler;
                return true;
            }

            return false;
        }

        protected virtual void Unsubscribe(T item) => ((INotifyPropertyChanged)item!).PropertyChanged -= _handler;

        protected virtual void OnCollectionChanged() => OnChanged(null, null);

        protected virtual void OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected void OnAdded(T item)
        {
            SubscribeIfNeed(item);
            OnCollectionChanged();
        }

        protected void OnReplaced(T oldItem, T newItem)
        {
            UnsubscribeIfNeed(oldItem);
            SubscribeIfNeed(newItem);
            OnCollectionChanged();
        }

        protected void OnMoved(T item) => OnCollectionChanged();

        protected void OnRemoved(T item)
        {
            UnsubscribeIfNeed(item);
            OnCollectionChanged();
        }

        protected void OnReset(IEnumerable<T>? oldItems, IEnumerable<T>? items)
        {
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
                _oldItems ??= new List<T>(_items.Count);
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

        private void EndSuspend()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
            {
                _isNotificationsDirty = false;
                OnChanged(null, null);
            }
        }

        private void RemoveObserver(Observer observer)
        {
            bool removed;
            lock (_observers)
            {
                removed = _observers.Remove(observer);
            }

            if (removed)
                observer.Dispose();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => OnChanged((T?)sender, args.PropertyName ?? "");

        private void SubscribeIfNeed(T? item)
        {
            if (item == null)
                return;

            if (_items.TryGetValue(item, out var count) || TrySubscribe(item))
                _items[item] = count + 1;
        }

        private void UnsubscribeIfNeed(T? item)
        {
            if (item == null || !_items.TryGetValue(item, out var count))
                return;
            if (--count == 0)
            {
                Unsubscribe(item);
                _items.Remove(item);
            }
            else
                _items[item] = count - 1;
        }

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            foreach (var item in (IReadOnlyCollection<T>)owner)
                SubscribeIfNeed(item);
            if (_items.Count != 0)
                OnCollectionChanged();
            OnAttached(owner, metadata);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            foreach (var item in _items)
                Unsubscribe(item.Key);
            _items.Clear();
            OnDetached(owner, metadata);
        }

        void IDisposableComponent<IReadOnlyObservableCollection>.Dispose(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => ClearObservers();

        bool ISuspendableComponent<IReadOnlyObservableCollection>.IsSuspended(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => _suspendCount != 0;

        ActionToken ISuspendableComponent<IReadOnlyObservableCollection>.TrySuspend(IReadOnlyObservableCollection owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate(this, t => t.EndSuspend());
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct ChangedEventInfo
        {
            public readonly T? Item;
            public readonly string? Member;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ChangedEventInfo(T? item, string? member)
            {
                Item = item;
                Member = member;
            }

            public bool IsCollectionEvent => Item == null && Member == null;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsMemberChanged(string member, bool emptyMemberResult = true) => Item != null && (member == Member || Member == "" && emptyMemberResult);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsMemberOrCollectionChanged(string member, bool emptyMemberResult = true) => Item == null || member == Member || Member == "" && emptyMemberResult;
        }

        private abstract class Observer : IDisposable
        {
            public abstract void OnChanged(T? item, string? member);

            public abstract void Dispose();
        }

        private sealed class Observer<TState> : Observer
        {
            private readonly TState _state;
            private readonly Action<TState, T?> _invokeAction;
            private readonly Func<TState, ChangedEventInfo, bool> _canInvoke;
            private readonly int _delay;
            private Timer? _timer;
            private T? _item;

            public Observer(TState state, Func<TState, ChangedEventInfo, bool> canInvoke, Action<TState, T?> invokeAction, int delay)
            {
                _state = state;
                _invokeAction = invokeAction;
                _canInvoke = canInvoke;
                _delay = delay;
                if (delay != 0)
                    _timer = new Timer(o => ((Observer<TState>)o!).Raise(), this, Timeout.Infinite, Timeout.Infinite);
            }

            public override void OnChanged(T? item, string? member)
            {
                if (!_canInvoke(_state, new ChangedEventInfo(item, member)))
                    return;

                if (_delay == 0)
                    _invokeAction(_state, item);
                else
                {
                    _item = item;
                    _timer?.Change(_delay, Timeout.Infinite);
                }
            }

            public override void Dispose()
            {
                _timer?.Dispose();
                _timer = null;
            }

            private void Raise()
            {
                _invokeAction(_state, _item);
                _item = null;
            }
        }
    }
}