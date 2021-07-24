﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public abstract class FlattenCollectionDecorator : CollectionDecoratorBase
    {
        private int? _batchLimit;

        internal FlattenCollectionDecorator(int priority) : base(priority)
        {
        }

        public int BatchLimit
        {
            get => _batchLimit.GetValueOrDefault(CollectionMetadata.FlattenCollectionDecoratorBatchLimit);
            set => _batchLimit = value;
        }

        internal abstract int GetIndex(int originalIndex);

        internal abstract IEnumerable<object?> Decorate(IEnumerable<object?> items);
    }

    public class FlattenCollectionDecorator<T> : FlattenCollectionDecorator, ILockerChangedListener<IReadOnlyObservableCollection>,
        ICollectionItemPreInitializerComponent<object>
        where T : class
    {
        private readonly Func<T, FlattenItemInfo> _getNestedCollection;
        private readonly Dictionary<T, FlattenCollectionItemBase> _collectionItems;

        public FlattenCollectionDecorator(Func<T, FlattenItemInfo> getNestedCollection, int priority = CollectionComponentPriority.FlattenCollectionDecorator)
            : base(priority)
        {
            Should.NotBeNull(getNestedCollection, nameof(getNestedCollection));
            _getNestedCollection = getNestedCollection;
            Priority = priority;
            _collectionItems = new Dictionary<T, FlattenCollectionItemBase>(InternalEqualityComparer.Reference);
        }

        public override bool HasAdditionalItems => _collectionItems.Count > 0;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, ref ItemOrListEditor<int> indexes)
        {
            foreach (var collectionItem in _collectionItems)
                collectionItem.Value.FindAllIndexOf(item, ref indexes);
            return true;
        }

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (item is T itemT)
            {
                var flattenItemInfo = _getNestedCollection(itemT);
                _collectionItems.TryGetValue(itemT, out var flattenItem);
                if (!ReferenceEquals(flattenItemInfo.Items, flattenItem?.Collection))
                    return OnReplaced(decoratorManager, collection, ref item, ref item, ref index);

                if (flattenItem != null)
                    return false;
            }

            index = GetIndex(index);
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index) =>
            OnAdded(collection, item, ref index, true, true, out _);

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            using var t = decoratorManager.BatchUpdate(collection, this);
            var addIndex = index;
            var removed = OnRemoved(oldItem, ref index, out var isRemoveReset);
            if (removed)
            {
                if (newItem is T newItemT && !_getNestedCollection(newItemT).IsEmpty)
                {
                    decoratorManager.OnRemoved(collection, this, oldItem, index);
                    removed = false;
                }
            }

            var added = OnAdded(collection, newItem, ref addIndex, !isRemoveReset, true, out var isAddReset);
            if (added && removed)
                return true;
            if (isAddReset || isRemoveReset)
                return false;

            if (removed)
                decoratorManager.OnRemoved(collection, this, oldItem, index);
            if (added)
                decoratorManager.OnAdded(collection, this, newItem, index);
            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (item is T itemT && TryGetCollectionItem(itemT, out var flattenCollectionItem))
                flattenCollectionItem.Indexes.Remove(oldIndex);
            else
                flattenCollectionItem = null;

            var originalNewIndex = newIndex;
            oldIndex = UpdateIndexes(oldIndex, -1);
            newIndex = UpdateIndexes(newIndex, 1);
            if (flattenCollectionItem == null)
                return true;

            flattenCollectionItem.Indexes.Add(originalNewIndex);
            if (oldIndex != newIndex)
                flattenCollectionItem.OnMoved(oldIndex, newIndex);
            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index) =>
            OnRemoved(item, ref index, out _);

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items == null)
                Clear();
            else
            {
                foreach (var collectionItem in _collectionItems)
                    collectionItem.Value.Indexes.Clear();

                var index = 0;
                var i = 0;
                foreach (var item in items)
                {
                    OnAdded(collection, item, ref i, false, false, out _);
                    i = ++index;
                }

#if !NET5_0
                var toRemove = new ItemOrListEditor<T>(2);
#endif
                foreach (var item in _collectionItems)
                {
                    if (item.Value.Indexes.Count == 0)
                    {
#if NET5_0
                        _collectionItems.Remove(item.Key);
#else
                        toRemove.Add(item.Key);
#endif
                        item.Value.Detach();
                    }
                }
#if !NET5_0
                foreach (var item in toRemove)
                    _collectionItems.Remove(item);
#endif

                items = Decorate(items);
            }

            return true;
        }

        internal sealed override IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var item in items)
            {
                if (item is T itemT)
                {
                    var flattenItemInfo = _getNestedCollection(itemT);
                    if (!flattenItemInfo.IsEmpty)
                    {
                        foreach (var nestedItem in flattenItemInfo.GetItems())
                            yield return nestedItem;
                        continue;
                    }
                }

                yield return item;
            }
        }

        internal sealed override int GetIndex(int originalIndex)
        {
            var result = originalIndex;
            foreach (var collectionItem in _collectionItems)
            {
                var item = collectionItem.Value;
                var items = item.Indexes.Items;
                var count = item.Indexes.Count;
                for (var i = 0; i < count; i++)
                {
                    if (items[i] < originalIndex)
                        result += item.Size - 1;
                }
            }

            return result;
        }

        private int UpdateIndexes(int index, int value)
        {
            var result = index;
            foreach (var collectionItem in _collectionItems)
            {
                var item = collectionItem.Value;
                var items = item.Indexes.Items;
                var count = item.Indexes.Count;
                for (var i = 0; i < count; i++)
                {
                    if (items[i] >= index)
                        items[i] += value;
                    else
                        result += item.Size - 1;
                }
            }

            return result;
        }

        private void Clear()
        {
            foreach (var collectionItem in _collectionItems)
                collectionItem.Value.Detach();
            _collectionItems.Clear();
        }

        private bool OnAdded(object source, object? item, ref int index, bool notify, bool updateIndex, out bool isReset)
        {
            var originalIndex = index;
            if (updateIndex)
                index = UpdateIndexes(index, 1);
            if (item is not T itemT)
            {
                isReset = false;
                return true;
            }

            var isRecycled = true;
            if (!_collectionItems.TryGetValue(itemT, out var flattenCollectionItem))
            {
                var flattenItemInfo = _getNestedCollection(itemT);
                if (flattenItemInfo.IsEmpty)
                {
                    isReset = false;
                    return true;
                }

                flattenCollectionItem = flattenItemInfo.GetCollectionItem(this);
                _collectionItems[itemT] = flattenCollectionItem;
                isRecycled = false;
            }

            flattenCollectionItem.OnAdded(source, originalIndex, index, notify, isRecycled, out isReset);
            return false;
        }

        private bool OnRemoved(object? item, ref int index, out bool isReset)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, -1);
            if (item is not T itemT || !TryGetCollectionItem(itemT, out var flattenCollectionItem))
            {
                isReset = false;
                return true;
            }

            if (flattenCollectionItem.OnRemoved(originalIndex - 1, index, out isReset))
                _collectionItems.Remove(itemT);
            return false;
        }

        private bool TryGetCollectionItem(T item, [NotNullWhen(true)] out FlattenCollectionItemBase? value)
        {
            if (_collectionItems.TryGetValue(item, out value))
                return true;
            value = null;
            return false;
        }

        void ICollectionItemPreInitializerComponent<object>.Initialize(IReadOnlyObservableCollection<object> collection, object item)
        {
            if (item is not T itemT)
                return;

            var flattenItemInfo = _getNestedCollection(itemT);
            if (flattenItemInfo.IsEmpty)
                return;

            using var _ = MugenExtensions.TryLock(flattenItemInfo.Items);
            if (collection is ISynchronizable owner && flattenItemInfo.Items is ISynchronizable coll)
            {
                owner.UpdateLocker(coll.Locker);
                coll.UpdateLocker(owner.Locker);
            }
        }

        void ILockerChangedListener<IReadOnlyObservableCollection>.OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            using var _ = owner.TryLock();
            foreach (var item in _collectionItems)
                item.Value.UpdateLocker(locker);
        }
    }
}