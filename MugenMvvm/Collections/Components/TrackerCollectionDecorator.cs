using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    public class TrackerCollectionDecorator<T, TState> : CollectionDecoratorBase, IListenerCollectionDecorator where T : notnull
    {
        private readonly Dictionary<T, (TState state, int count)> _items;
        private readonly Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState?, int, bool, TState> _onAdded;
        private readonly Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, TState> _onRemoved;
        private readonly Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, object?, TState>? _onChanged;
        private readonly Action<IReadOnlyDictionary<T, (TState state, int count)>>? _onReset;
        private Dictionary<T, int>? _resetItems;

        public TrackerCollectionDecorator(Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState?, int, bool, TState> onAdded,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, TState> onRemoved,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, object?, TState>? onChanged,
            Action<IReadOnlyDictionary<T, (TState state, int count)>>? onReset,
            IEqualityComparer<T>? comparer = null, int priority = CollectionComponentPriority.TrackerDecorator) : base(priority)
        {
            Should.NotBeNull(onAdded, nameof(onAdded));
            Should.NotBeNull(onRemoved, nameof(onRemoved));
            _onAdded = onAdded;
            _onRemoved = onRemoved;
            _onChanged = onChanged;
            _onReset = onReset;
            _items = new Dictionary<T, (TState, int)>(comparer ?? EqualityComparer<T>.Default);
        }

        protected override bool IsLazy => false;

        protected override bool HasAdditionalItems => false;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (_onChanged != null && item is T itemT && _items.TryGetValue(itemT, out var state))
            {
                var newState = _onChanged(_items, itemT, state.state, state.count, false, args);
                if (!EqualityComparer<TState>.Default.Equals(newState, state.state))
                    _items[itemT] = (newState, state.count);
            }

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (item is T itemT)
                Add(itemT, false);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (oldItem is T oldItemT)
            {
                if (newItem is T newItemT)
                {
                    if (_items.Comparer.Equals(oldItemT, newItemT))
                        return true;
                    Add(newItemT, false);
                }

                Remove(oldItemT);
                return true;
            }

            if (newItem is T nT)
                Add(nT, false);

            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex) => true;

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (item is T itemT)
                Remove(itemT);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items == null)
                Clear();
            else
            {
                if (_items.Count == 0)
                {
                    foreach (var item in items)
                    {
                        if (item is T itemT)
                            Add(itemT, true);
                    }
                }
                else
                {
                    if (_resetItems == null)
                        _resetItems = new Dictionary<T, int>(_items.Count, _items.Comparer);
                    else
                        _resetItems.Clear();

                    foreach (var item in items)
                    {
                        if (item is not T itemT)
                            continue;
                        _resetItems.TryGetValue(itemT, out var c);
                        _resetItems[itemT] = c + 1;
                    }

                    foreach (var newItem in _resetItems)
                    {
                        if (_items.TryGetValue(newItem.Key, out var state))
                        {
                            if (state.count > newItem.Value)
                                _items[newItem.Key] = (Remove(newItem.Key, state.state, state.count, state.count - newItem.Value), newItem.Value);
                            else if (state.count < newItem.Value)
                                _items[newItem.Key] = (Add(newItem.Key, state.state, state.count, newItem.Value - state.count), newItem.Value);
                            else if (_onChanged != null)
                                _items[newItem.Key] = (_onChanged(_items, newItem.Key, state.state, state.count, true, null), state.count);
                        }
                        else
                            _items[newItem.Key] = (Add(newItem.Key, default, 0, newItem.Value), newItem.Value);
                    }

                    foreach (var item in _items)
                    {
                        if (_resetItems.ContainsKey(item.Key))
                            continue;

#if NET5_0
                        _items.Remove(item.Key);
                        Remove(item.Key, item.Value.state, item.Value.count, item.Value.count);
#else
                        _resetItems[item.Key] = -1;
#endif
                    }

#if !NET5_0
                    foreach (var item in _resetItems)
                    {
                        if (item.Value == -1 && _items.Remove(item.Key, out var state))
                            Remove(item.Key, state.state, state.count, state.count);
                    }
#endif
                    _resetItems.Clear();
                }

                _onReset?.Invoke(_items);
            }

            return true;
        }

        private void Add(T item, bool isReset)
        {
            _items.TryGetValue(item, out var state);
            _items[item] = (_onAdded(_items, item, state.state, state.count + 1, isReset), state.count + 1);
        }

        private void Remove(T item)
        {
            var state = _items[item];
            var count = state.count - 1;
            if (count == 0)
            {
                _items.Remove(item);
                _onRemoved(_items, item, state.state, 0, false);
            }
            else
                _items[item] = (_onRemoved(_items, item, state.state, count, false), count);
        }

        private void Clear()
        {
            if (_items.Count == 0)
                return;

#if NET5_0
            foreach (var item in _items)
            {
                _items.Remove(item.Key);
                Remove(item.Key, item.Value.state, item.Value.count, item.Value.count);
            }
#else
            _resetItems ??= new Dictionary<T, int>(_items.Count, _items.Comparer);
            foreach (var item in _items)
                _resetItems[item.Key] = 0;

            foreach (var item in _resetItems)
            {
                if (_items.Remove(item.Key, out var state))
                    Remove(item.Key, state.state, state.count, state.count);
            }

            _resetItems.Clear();
#endif
            _onReset?.Invoke(_items);
        }

        private TState Add(T item, TState? state, int count, int addCount)
        {
            for (var i = 0; i < addCount; i++)
                state = _onAdded(_items, item, state, count + i + 1, true);
            return state!;
        }

        private TState Remove(T item, TState state, int count, int removeCount)
        {
            for (var i = 0; i < removeCount; i++)
                state = _onRemoved(_items, item, state, count - i - 1, true);
            return state;
        }
    }
}