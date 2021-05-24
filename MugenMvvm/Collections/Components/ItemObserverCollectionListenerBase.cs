using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public abstract class ItemObserverCollectionListenerBase<T> : IAttachableComponent, IDetachableComponent, IHasPriority where T : class?
    {
        private readonly Dictionary<T, int> _items;
        private readonly PropertyChangedEventHandler _handler;
        private readonly ListInternal<Observer> _observers;
#if NET461
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
            var listener = new Observer<TState>(state, canInvoke, invokeAction, delay);
            _observers.Add(listener);
            return ActionToken.FromDelegate((l, item) => ((List<Observer>) l!).Remove((Observer) item!), _observers, listener);
        }

        public void ClearObservers() => _observers.Clear();

        protected virtual void OnChanged(T? item, string? member)
        {
            var items = _observers.Items;
            var count = _observers.Count;
            for (var i = 0; i < count; i++)
                items[i].OnChanged(item, member);
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

        protected virtual void Unsubscribe(T item) => ((INotifyPropertyChanged) item!).PropertyChanged -= _handler;

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
#if NET461
                _oldItems ??= new List<T>(_items.Count);
                foreach (var item in _items)
                {
                    for (int i = 0; i < item.Value; i++)
                        _oldItems.Add(item.Key);
                }

                oldItems = _oldItems;
#else
                foreach (var item in _items)
                    _items[item.Key] = 0;
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

            OnCollectionChanged();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => OnChanged((T?) sender, args.PropertyName);

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
            foreach (var item in (IReadOnlyCollection<T>) owner)
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
            public bool IsMemberChanged(string member) => Item != null && Member == member;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsMemberOrCollectionChanged(string member) => Item == null || Member == member;
        }

        private abstract class Observer
        {
            public abstract void OnChanged(T? item, string? member);
        }

        private sealed class Observer<TState> : Observer
        {
            private readonly TState _state;
            private readonly Action<TState, T?> _invokeAction;
            private readonly Func<TState, ChangedEventInfo, bool> _canInvoke;
            private readonly int _delay;
            private int _id;

            public Observer(TState state, Func<TState, ChangedEventInfo, bool> canInvoke, Action<TState, T?> invokeAction, int delay)
            {
                _state = state;
                _invokeAction = invokeAction;
                _canInvoke = canInvoke;
                _delay = delay;
            }

            public override async void OnChanged(T? item, string? member)
            {
                if (!_canInvoke(_state, new ChangedEventInfo(item, member)))
                    return;

                if (_delay == 0)
                    _invokeAction(_state, item);
                else
                {
                    var id = Interlocked.Increment(ref _id);
                    await Task.Delay(_delay).ConfigureAwait(false);
                    if (_id == id)
                        _invokeAction(_state, item);
                }
            }
        }
    }
}