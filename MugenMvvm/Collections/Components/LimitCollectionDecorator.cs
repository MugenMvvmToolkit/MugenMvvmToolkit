using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class LimitCollectionDecorator<T> : CollectionDecoratorBase
    {
        private const int NotFound = -1;

        private IndexMapList<object?> _items;
        private Func<T, bool>? _condition;
        private int? _limit;

        public LimitCollectionDecorator(int? limit = null, Func<T, bool>? condition = null, int priority = CollectionComponentPriority.LimitDecorator) : base(priority)
        {
            _items = IndexMapList<object?>.Get();
            _condition = condition;
            _limit = limit;
            Priority = priority;
        }

        public override bool HasAdditionalItems => false;

        public Func<T, bool>? Condition
        {
            get => _condition;
            set
            {
                if (_condition != value)
                    Update(value, _limit);
            }
        }

        public int? Limit
        {
            get => _limit;
            set
            {
                if (_limit != value)
                    Update(_condition, value);
            }
        }

        public int Count => _items.Size;

        private bool HasLimit => Limit != null;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _items.Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => HasLimit ? Decorate(items) : items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (!HasLimit)
                return true;

            var currentIndex = _items.IndexOfKey(index);
            var oldSatisfied = currentIndex != NotFound;
            var newSatisfied = IsSatisfied(item);

            if (oldSatisfied == newSatisfied)
            {
                if (oldSatisfied && currentIndex >= Limit)
                    return false;

                index = GetIndex(index);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, newSatisfied, item, item, index);
            return false;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            if (IsSatisfied(item))
            {
                if (!Add(decoratorManager, item, index))
                    return false;
            }
            else
                _items.UpdateIndexes(index, 1);

            index = GetIndex(index);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (!HasLimit)
                return true;

            var oldSatisfied = IsSatisfied(oldItem);
            var newSatisfied = IsSatisfied(newItem);

            if (!oldSatisfied && !newSatisfied)
            {
                index = GetIndex(index);
                return true;
            }

            Replace(decoratorManager, collection, oldSatisfied, newSatisfied, oldItem, newItem, index);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!HasLimit)
                return true;

            if (oldIndex < Limit && newIndex < Limit || !IsSatisfied(item))
            {
                var toRemove = _items.IndexOfKey(oldIndex);
                _items.UpdateIndexes(oldIndex + 1, -1);
                _items.UpdateIndexes(newIndex, 1);
                if (toRemove != NotFound)
                {
                    _items.RemoveAt(toRemove);
                    _items.Add(newIndex, item);
                }

                oldIndex = GetIndex(oldIndex, oldIndex > newIndex);
                newIndex = GetIndex(newIndex);
                return true;
            }

            Remove(decoratorManager, item, oldIndex);
            Add(decoratorManager, item, newIndex);
            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!HasLimit)
                return true;

            if (IsSatisfied(item))
            {
                if (!Remove(decoratorManager, item, index))
                    return false;
            }
            else
                _items.UpdateIndexes(index, -1);

            index = GetIndex(index);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (!HasLimit)
                return true;

            _items.Clear();
            if (items != null)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            return true;
        }

        private void Update(Func<T, bool>? condition, int? limit)
        {
            if (DecoratorManager == null)
            {
                _condition = condition;
                _limit = limit;
                return;
            }

            using var _ = Owner.TryLock();
            _condition = condition;
            _limit = limit;
            _items.Clear();

            var items = DecoratorManager.Decorate(Owner, this);
            if (HasLimit)
            {
                UpdateItems(items);
                items = Decorate(items);
            }

            DecoratorManager.OnReset(Owner, this, items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSatisfied(object? item)
        {
            if (_condition == null)
                return item is T;
            return item is T t && _condition(t);
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, bool oldSatisfied, bool newSatisfied, object? oldItem,
            object? newItem, int index)
        {
            if (!oldSatisfied || Remove(decoratorManager, oldItem, index))
            {
                if (!oldSatisfied)
                    _items.UpdateIndexes(index, -1);
                decoratorManager.OnRemoved(collection, this, oldItem, GetIndex(index));
            }

            if (!newSatisfied || Add(decoratorManager, newItem, index))
            {
                if (!newSatisfied)
                    _items.UpdateIndexes(index, 1);
                decoratorManager.OnAdded(collection, this, newItem, GetIndex(index));
            }
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            var count = 0;
            foreach (var item in items)
            {
                if (IsSatisfied(item) && ++count > Limit)
                    continue;

                yield return item;
            }
        }

        private void UpdateItems(IEnumerable<object?> items)
        {
            if (items is IReadOnlyCollection<object?> c)
                _items.EnsureCapacity(c.Count);

            var index = 0;
            foreach (var item in items)
            {
                if (IsSatisfied(item))
                    _items.AddRaw(index, item);
                ++index;
            }
        }

        private bool Add(ICollectionDecoratorManagerComponent decoratorManager, object? item, int index)
        {
            var binarySearchIndex = _items.BinarySearch(index);
            _items.UpdateIndexes(index, 1, binarySearchIndex);
            var newIndex = _items.Add(index, item, binarySearchIndex);
            var limit = Limit!.Value;
            if (limit == 0 || newIndex >= limit)
                return false;

            if (_items.Size <= limit)
                return true;

            var oldIndex = _items.Keys[limit];
            var oldItem = _items.Values[limit];
            if (index == oldIndex - 1)
            {
                decoratorManager.OnReplaced(Owner, this, oldItem, item, GetIndex(index));
                return false;
            }

            decoratorManager.OnAdded(Owner, this, item, GetIndex(index));
            decoratorManager.OnRemoved(Owner, this, oldItem, GetIndex(oldIndex));
            return false;
        }

        private bool Remove(ICollectionDecoratorManagerComponent decoratorManager, object? item, int index)
        {
            var indexToRemove = _items.BinarySearch(index);
            _items.UpdateIndexes(index, -1, indexToRemove);
            _items.RemoveAt(indexToRemove);
            var limit = Limit!.Value;
            if (limit == 0 || indexToRemove >= limit)
                return false;

            if (_items.Size + 1 <= limit)
                return true;

            var oldIndex = _items.Keys[limit - 1];
            var oldItem = _items.Values[limit - 1];
            if (index == oldIndex)
            {
                decoratorManager.OnReplaced(Owner, this, item, oldItem, GetIndex(index));
                return false;
            }

            decoratorManager.OnRemoved(Owner, this, item, GetIndex(index));
            decoratorManager.OnAdded(Owner, this, oldItem, GetIndex(oldIndex));

            return false;
        }

        private int GetIndex(int index, bool isMove = false)
        {
            if (Limit!.Value > index || Limit!.Value >= _items.Size)
                return index;

            var lastIndex = _items.BinarySearch(index);
            if (lastIndex == 0)
                return index;
            if (lastIndex < 0)
                lastIndex = ~lastIndex;
            else if (isMove)
                ++lastIndex;
            if (lastIndex <= Limit.Value)
                return index;
            return index - (lastIndex - Limit.Value);
        }
    }
}