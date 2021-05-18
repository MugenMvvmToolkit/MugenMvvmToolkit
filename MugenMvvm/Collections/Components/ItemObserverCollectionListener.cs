using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public class ItemObserverCollectionListener<T> : ICollectionChangingListener<T>, IDetachableComponent, IHasPriority where T : class
    {
        private readonly Dictionary<T, int> _items;
        private readonly PropertyChangedEventHandler _handler;
        private readonly List<Observer> _observers;

        public ItemObserverCollectionListener() : this(null)
        {
        }

        public ItemObserverCollectionListener(IEqualityComparer<T>? comparer)
        {
            _handler = OnPropertyChanged;
            _observers = new List<Observer>();
            _items = new Dictionary<T, int>(comparer ?? EqualityComparer<T>.Default);
        }

        public int Priority { get; set; }

        public ActionToken AddObserver<TState>(TState state, Func<TState, ChangedEventInfo, bool> canInvoke, Action<TState, T?> invokeAction, int delay = 0)
        {
            Should.NotBeNull(invokeAction, nameof(invokeAction));
            Should.NotBeNull(canInvoke, nameof(canInvoke));
            var listener = new Observer<TState>(state, canInvoke, invokeAction, delay);
            _observers.Add(listener);
            return new ActionToken((l, item) => ((List<Observer>) l!).Remove((Observer) item!), _observers, listener);
        }

        public void ClearObservers() => _observers.Clear();

        protected virtual void OnChanged(T? item, string? member)
        {
            for (var i = 0; i < _observers.Count; i++)
                _observers[i].OnChanged(item, member);
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

        protected virtual void Unsubscribe(T item) => ((INotifyPropertyChanged) item).PropertyChanged -= _handler;

        protected virtual void OnCollectionChanged() => OnChanged(null, null);

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

        void ICollectionChangingListener<T>.OnAdding(IReadOnlyCollection<T> collection, T item, int index)
        {
            SubscribeIfNeed(item);
            OnCollectionChanged();
        }

        void ICollectionChangingListener<T>.OnReplacing(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            UnsubscribeIfNeed(oldItem);
            SubscribeIfNeed(newItem);
            OnCollectionChanged();
        }

        void ICollectionChangingListener<T>.OnMoving(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex) => OnCollectionChanged();

        void ICollectionChangingListener<T>.OnRemoving(IReadOnlyCollection<T> collection, T item, int index)
        {
            UnsubscribeIfNeed(item);
            OnCollectionChanged();
        }

        void ICollectionChangingListener<T>.OnResetting(IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            if (items != null)
            {
                foreach (var item in items)
                    SubscribeIfNeed(item);
            }

            foreach (var item in collection)
                UnsubscribeIfNeed(item);

            OnCollectionChanged();
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            foreach (var item in _items)
                Unsubscribe(item.Key);
            _items.Clear();
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