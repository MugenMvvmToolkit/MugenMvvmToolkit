using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public sealed class TrackerCollectionDecorator<T, TState> : CollectionDecoratorBase, IListenerCollectionDecorator, ICollectionBatchUpdateListener, IReadOnlyCollection<T>,
        IEqualityComparer<NullableKey<T>>
    {
        internal readonly Dictionary<NullableKey<T>, (TState state, int count)> ItemsRaw;
        private readonly bool _allowNull;
        private readonly Func<TrackerCollectionDecorator<T, TState>, T, TState?, int, TState> _onAdded;
        private readonly Func<TrackerCollectionDecorator<T, TState>, T, TState, int, TState> _onRemoved;
        private readonly Func<TrackerCollectionDecorator<T, TState>, T, TState, int, object?, TState>? _onChanged;
        private readonly Action<TrackerCollectionDecorator<T, TState>>? _onEndBatchUpdate;
        private readonly Func<T, bool>? _condition;
        private readonly IEqualityComparer<T>? _comparer;
        private Dictionary<NullableKey<T>, int>? _resetItems;
        private bool _isBatchUpdate;

        private bool _hasItem;
        private T? _item;

        public TrackerCollectionDecorator(int priority, bool allowNull, Func<TrackerCollectionDecorator<T, TState>, T, TState?, int, TState> onAdded,
            Func<TrackerCollectionDecorator<T, TState>, T, TState, int, TState> onRemoved,
            Func<TrackerCollectionDecorator<T, TState>, T, TState, int, object?, TState>? onChanged,
            Action<TrackerCollectionDecorator<T, TState>>? onEndBatchUpdate, Func<T, bool>? immutableCondition = null,
            IEqualityComparer<T>? comparer = null) : base(priority)
        {
            Should.NotBeNull(onAdded, nameof(onAdded));
            Should.NotBeNull(onRemoved, nameof(onRemoved));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _onAdded = onAdded;
            _onRemoved = onRemoved;
            _onChanged = onChanged;
            _onEndBatchUpdate = onEndBatchUpdate;
            _condition = immutableCondition;
            _comparer = comparer;
            ItemsRaw = comparer == null ? new Dictionary<NullableKey<T>, (TState state, int count)>() : new Dictionary<NullableKey<T>, (TState state, int count)>(this);
        }

        public IReadOnlyDictionary<NullableKey<T>, (TState state, int count)> Items => ItemsRaw;

        public bool IsBatchUpdate => IsReset || _isBatchUpdate;

        public bool IsReset { get; private set; }

        public int Count
        {
            get
            {
                if (_hasItem)
                    return ItemsRaw.Count + 1;
                return ItemsRaw.Count;
            }
        }

        protected override bool IsLazy => false;

        protected override bool HasAdditionalItems => false;

        public bool Contains(NullableKey<T> item) => _hasItem && ItemsRaw.Comparer.Equals(_item, item) || ItemsRaw.ContainsKey(item);

        public T? FirstOrDefault()
        {
            if (_hasItem)
                return _item;
            return ItemsRaw.FirstOrDefault().Key.Value;
        }

        public ActionToken Lock()
        {
            var collection = OwnerOptional;
            if (collection == null)
                return default;
            return collection.Lock();
        }

        public void ForEach<TActionState>(TActionState state, Action<T, TState?, TActionState> action)
        {
            Should.NotBeNull(action, nameof(action));
            var collection = OwnerOptional;
            if (collection == null)
                return;
            using var _ = collection.Lock();
            if (OwnerOptional == null)
                return;
            if (_hasItem)
                action(_item!, default, state);
            foreach (var pair in ItemsRaw)
                action(pair.Key.Value!, pair.Value.state, state);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (ItemsRaw.Count == 0)
                return _hasItem ? Default.SingleItemEnumerator(_item!) : Default.Enumerator<T>();
            return Enumerate();
        }

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
            if (_onChanged != null && IsSatisfied(item, out var itemT) && ItemsRaw.TryGetValue(itemT, out var state))
            {
                if (IsReset)
                    ExceptionManager.ThrowCollectionWasModified(collection);
                var newState = _onChanged(this, itemT, state.state, state.count, args);
                if (!EqualityComparer<TState>.Default.Equals(newState, state.state))
                    ItemsRaw[itemT] = (newState, state.count);
            }

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (IsSatisfied(item, out var itemT))
            {
                if (IsReset)
                    ExceptionManager.ThrowCollectionWasModified(collection);
                Add(itemT);
            }

            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (IsSatisfied(oldItem, out var oldItemT))
            {
                if (IsReset)
                    ExceptionManager.ThrowCollectionWasModified(collection);
                if (IsSatisfied(newItem, out var newItemT))
                {
                    if (ItemsRaw.Comparer.Equals(oldItemT, newItemT))
                        return true;
                    Add(newItemT);
                }

                Remove(oldItemT);
                return true;
            }

            if (IsSatisfied(newItem, out var nT))
            {
                if (IsReset)
                    ExceptionManager.ThrowCollectionWasModified(collection);
                Add(nT);
            }

            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex) => true;

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (IsSatisfied(item, out var itemT))
            {
                if (IsReset)
                    ExceptionManager.ThrowCollectionWasModified(collection);
                Remove(itemT);
            }

            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (IsReset)
                ExceptionManager.ThrowCollectionWasModified(collection);
            IsReset = true;
            Reset(ref items);
            IsReset = false;
            return true;
        }

        private void Reset(ref IEnumerable<object?>? items)
        {
            if (items.IsNullOrEmpty())
                Clear();
            else
            {
                if (ItemsRaw.Count == 0)
                {
                    foreach (var item in items)
                    {
                        if (IsSatisfied(item, out var itemT))
                            Add(itemT);
                    }
                }
                else
                {
                    if (_resetItems == null)
                        _resetItems = new Dictionary<NullableKey<T>, int>(ItemsRaw.Count, ItemsRaw.Comparer);
                    else
                        _resetItems.Clear();

                    foreach (var item in items)
                    {
                        if (!IsSatisfied(item, out var itemT))
                            continue;
                        _resetItems.TryGetValue(itemT, out var c);
                        _resetItems[itemT] = c + 1;
                    }

                    foreach (var newItem in _resetItems)
                    {
                        if (ItemsRaw.TryGetValue(newItem.Key, out var state))
                        {
                            if (state.count > newItem.Value)
                                Remove(newItem.Key, state.state, state.count, state.count - newItem.Value);
                            else if (state.count < newItem.Value)
                                Add(newItem.Key, state.state, state.count, newItem.Value - state.count);
                            else if (_onChanged != null)
                                ItemsRaw[newItem.Key] = (_onChanged(this, newItem.Key.Value!, state.state, state.count, null), state.count);
                        }
                        else
                            Add(newItem.Key, default, 0, newItem.Value);
                    }

                    foreach (var item in ItemsRaw)
                    {
                        if (_resetItems.ContainsKey(item.Key))
                            continue;

#if NET5_0
                        Remove(item.Key, item.Value.state, item.Value.count, item.Value.count);
#else
                        _resetItems[item.Key] = -1;
#endif
                    }

#if !NET5_0
                    foreach (var item in _resetItems)
                    {
                        if (item.Value == -1 && ItemsRaw.TryGetValue(item.Key, out var state))
                            Remove(item.Key, state.state, state.count, state.count);
                    }
#endif
                    _resetItems.Clear();
                }

                if (!_isBatchUpdate)
                    _onEndBatchUpdate?.Invoke(this);
            }
        }

        private void Clear()
        {
            if (ItemsRaw.Count == 0)
                return;

#if NET5_0
            foreach (var item in ItemsRaw)
                Remove(item.Key, item.Value.state, item.Value.count, item.Value.count);
#else
            _resetItems ??= new Dictionary<NullableKey<T>, int>(ItemsRaw.Count, ItemsRaw.Comparer);
            foreach (var item in ItemsRaw)
                _resetItems[item.Key] = 0;

            foreach (var item in _resetItems)
            {
                if (ItemsRaw.TryGetValue(item.Key, out var state))
                    Remove(item.Key, state.state, state.count, state.count);
            }

            _resetItems.Clear();
#endif
            if (!_isBatchUpdate)
                _onEndBatchUpdate?.Invoke(this);
        }

        private void Add(NullableKey<T> item, TState? state, int count, int addCount)
        {
            for (var i = 0; i < addCount; i++)
            {
                var currentCount = count + i + 1;
                _hasItem = currentCount == 1;
                _item = item.Value!;
                state = _onAdded(this, _item, state, currentCount);
                ItemsRaw[item] = (state, currentCount);
                _hasItem = false;
                _item = default;
            }
        }

        private void Add(T item)
        {
            _hasItem = !ItemsRaw.TryGetValue(item, out var state);
            _item = item;
            ItemsRaw[item] = (_onAdded(this, item, state.state, state.count + 1), state.count + 1);
            _hasItem = false;
            _item = default;
        }

        private void Remove(T item)
        {
            var state = ItemsRaw[item];
            var count = state.count - 1;
            if (count == 0)
            {
                ItemsRaw.Remove(item);
                _onRemoved(this, item, state.state, 0);
            }
            else
                ItemsRaw[item] = (_onRemoved(this, item, state.state, count), count);
        }

        private void Remove(NullableKey<T> item, TState state, int count, int removeCount)
        {
            for (var i = 0; i < removeCount; i++)
            {
                var currentCount = count - i - 1;
                if (currentCount == 0)
                    ItemsRaw.Remove(item);
                state = _onRemoved(this, item.Value!, state, currentCount);
                if (currentCount != 0)
                    ItemsRaw[item] = (state, currentCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item, [MaybeNullWhen(false)] out T result) => item.TryCast(_allowNull, out result!) && (_condition == null || _condition(result));

        private IEnumerator<T> Enumerate()
        {
            if (_hasItem)
                yield return _item!;
            foreach (var item in ItemsRaw)
                yield return item.Key.Value!;
        }

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Decorators)
                _isBatchUpdate = true;
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Decorators && _isBatchUpdate)
            {
                _isBatchUpdate = false;
                _onEndBatchUpdate?.Invoke(this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool IEqualityComparer<NullableKey<T>>.Equals(NullableKey<T> x, NullableKey<T> y) => _comparer!.Equals(x.Value!, y.Value!);

        int IEqualityComparer<NullableKey<T>>.GetHashCode(NullableKey<T> obj) => _comparer!.GetHashCode(obj.Value!);
    }
}