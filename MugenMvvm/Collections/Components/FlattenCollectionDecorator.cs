using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private IWeakReference? _weakReference;
        private int? _batchThreshold;

        internal FlattenCollectionDecorator(int priority) : base(priority)
        {
        }

        public int BatchThreshold
        {
            get => _batchThreshold.GetValueOrDefault(CollectionMetadata.FlattenCollectionDecoratorBatchThreshold);
            set => _batchThreshold = value;
        }

        protected override bool IsLazy => false;

        protected override bool IsCacheRequired => true;

        internal IWeakReference WeakReference => _weakReference ??= this.ToWeakReference();

        internal abstract int GetIndex(int originalIndex);

        internal abstract IEnumerable<object?> Decorate(IEnumerable<object?> items);
    }

    public sealed class FlattenCollectionDecorator<T> : FlattenCollectionDecorator, ILockerChangedListener<IReadOnlyObservableCollection>
        where T : class
    {
        private readonly Func<T, FlattenItemInfo> _getNestedCollection;
        private IndexMapAwareList<FlattenCollectionItemBase> _collectionItems;
        private Dictionary<object, object?>? _resetCache;

        public FlattenCollectionDecorator(int priority, Func<T, FlattenItemInfo> getNestedCollection)
            : base(priority)
        {
            Should.NotBeNull(getNestedCollection, nameof(getNestedCollection));
            _getNestedCollection = getNestedCollection;
            _collectionItems = IndexMapAwareList<FlattenCollectionItemBase>.Get();
        }

        protected override bool HasAdditionalItems => _collectionItems.Size > 0;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            base.OnDetached(owner, metadata);
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (item is T)
                return Replace(decoratorManager, collection, item, item, ref index);

            index = GetIndex(index);
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, 1, out var binarySearchIndex);
            if (item is not T itemT)
                return true;

            var flattenItemInfo = _getNestedCollection(itemT);
            if (flattenItemInfo.IsEmpty)
                return true;

            var flattenCollectionItem = flattenItemInfo.GetCollectionItem(itemT, this);
            _collectionItems.Add(originalIndex, flattenCollectionItem, binarySearchIndex);
            flattenCollectionItem.OnAdded(this, decoratorManager, collection, index, true, false, out _);
            return false;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index) =>
            Replace(decoratorManager, collection, oldItem, newItem, ref index);

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (!UpdateIndexesMove(ref oldIndex, ref newIndex, out var flattenCollectionItem))
                return true;

            if (oldIndex != newIndex)
                flattenCollectionItem.OnMoved(this, decoratorManager, collection, oldIndex, newIndex);
            return false;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            index = UpdateIndexes(index, -1, out var binarySearchIndex);
            if (binarySearchIndex < 0)
                return true;

            var flattenCollectionItemBase = _collectionItems.Indexes[binarySearchIndex].Value;
            _collectionItems.RemoveAt(binarySearchIndex);
            flattenCollectionItemBase.OnRemoved(this, decoratorManager, collection, index, out _);
            return false;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items.IsNullOrEmpty())
                Clear();
            else
            {
                if (_resetCache == null)
                    _resetCache = new Dictionary<object, object?>(_collectionItems.Size, InternalEqualityComparer.Reference);
                else
                    _resetCache.Clear();
                for (var i = 0; i < _collectionItems.Size; i++)
                {
                    var value = _collectionItems.Indexes[i].Value;
                    if (_resetCache.TryGetValue(value.Item, out var v))
                    {
                        var rawValue = ItemOrListEditor<FlattenCollectionItemBase>.FromRawValue(v);
                        rawValue.Add(value);
                        _resetCache[value.Item] = rawValue.GetRawValueInternal();
                    }
                    else
                        _resetCache[value.Item] = value;
                }

                _collectionItems.Clear();
                var index = 0;
                foreach (var item in items)
                {
                    OnAdded(decoratorManager, collection, item, index, _resetCache);
                    ++index;
                }

                foreach (var item in _resetCache)
                foreach (var collectionItemBase in ItemOrIReadOnlyList.FromRawValue<FlattenCollectionItemBase>(item.Value))
                    collectionItemBase.Detach();
                _resetCache.Clear();

                items = Decorate(items);
            }

            return true;
        }

        internal override IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            if (_collectionItems.Size == 0)
                return items;
            return DecorateImpl(items);
        }

        internal override int GetIndex(int originalIndex) => GetIndex(originalIndex, _collectionItems.BinarySearch(originalIndex));

        private IEnumerable<object?> DecorateImpl(IEnumerable<object?> items)
        {
            var index = 0;
            var itemIndex = 0;
            foreach (var item in items)
            {
                if (itemIndex < _collectionItems.Size && _collectionItems.Indexes[itemIndex].Index == index)
                {
                    int count = 0;
                    var flattenItem = _collectionItems.Indexes[itemIndex].Value;
                    foreach (var nestedItem in flattenItem.GetItems())
                    {
                        if (count < flattenItem.Size)
                            yield return nestedItem;
                        ++count;
                    }

                    ++itemIndex;
                    ++index;
                }
                else
                {
                    ++index;
                    yield return item;
                }
            }
        }

        private bool Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? oldItem, object? newItem, ref int index)
        {
            var oldIndex = _collectionItems.BinarySearch(index);
            var flattenItemInfo = newItem is T newItemT ? _getNestedCollection(newItemT) : default;

            if (oldIndex < 0)
            {
                if (flattenItemInfo.IsEmpty)
                {
                    index = GetIndex(index, oldIndex);
                    return true;
                }

                var originalIndex = index;
                index = GetIndex(index, oldIndex);
                var newFlattenItem = flattenItemInfo.GetCollectionItem(newItem!, this);
                _collectionItems.Add(originalIndex, newFlattenItem, oldIndex);
                newFlattenItem.OnAdded(this, decoratorManager, collection, index, true, false, out _, oldItem, true);
                return false;
            }

            var currentFlattenItem = _collectionItems.Indexes[oldIndex].Value;
            if (flattenItemInfo.IsEmpty)
            {
                index = GetIndex(index, oldIndex);
                _collectionItems.RemoveAt(oldIndex);
                currentFlattenItem.OnRemoved(this, decoratorManager, collection, index, out _, newItem, true);
                return false;
            }

            if (ReferenceEquals(flattenItemInfo.Items, currentFlattenItem.Collection))
            {
                currentFlattenItem.Item = newItem!;
                return false;
            }

            index = GetIndex(index, oldIndex);
            var replaceFlattenItem = flattenItemInfo.GetCollectionItem(newItem!, this);
            _collectionItems.Indexes[oldIndex].Value = replaceFlattenItem;
            replaceFlattenItem.OnReplaced(this, decoratorManager, collection, index, currentFlattenItem);
            return false;
        }

        private int GetIndex(int originalIndex, int binarySearchIndex)
        {
            if (binarySearchIndex < 0)
                binarySearchIndex = ~binarySearchIndex;
            for (var i = 0; i < binarySearchIndex; i++)
                originalIndex += _collectionItems.Indexes[i].Value.Size - 1;
            return originalIndex;
        }

        private bool UpdateIndexesMove(ref int oldIndex, ref int newIndex, [NotNullWhen(true)] out FlattenCollectionItemBase? item)
        {
            int? oldIndexBinary = _collectionItems.BinarySearch(oldIndex);
            int? newIndexBinary = _collectionItems.BinarySearch(newIndex);
            var oldIndexOriginal = oldIndex;
            var newIndexOriginal = newIndex;
            oldIndex = GetIndex(oldIndex, oldIndexBinary.Value);
            if (oldIndexOriginal > newIndexOriginal)
                newIndex = GetIndex(newIndex, newIndexBinary.Value);
            var result = _collectionItems.Move(oldIndexOriginal, newIndexOriginal, out item, ref oldIndexBinary, ref newIndexBinary);
            if (oldIndexOriginal < newIndexOriginal)
            {
                newIndex = GetIndex(newIndex, newIndexBinary.Value + (newIndexBinary.Value < 0 ? 0 : 1));
                if (item != null)
                    newIndex -= item.Size - 1;
            }

            return result;
        }

        private int UpdateIndexes(int index, int value, out int binarySearchIndex)
        {
            binarySearchIndex = _collectionItems.BinarySearch(index);
            _collectionItems.UpdateIndexesBinary(binarySearchIndex, value);
            int endIndex;
            if (binarySearchIndex < 0)
                endIndex = ~binarySearchIndex;
            else
                endIndex = binarySearchIndex;
            for (var i = 0; i < endIndex; i++)
                index += _collectionItems.Indexes[i].Value.Size - 1;
            return index;
        }

        private void Clear()
        {
            for (var i = 0; i < _collectionItems.Size; i++)
                _collectionItems.Indexes[i].Value.Detach();
            _collectionItems.Clear();
        }

        private void OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, object? item, int index,
            Dictionary<object, object?> cache)
        {
            if (item is not T itemT)
                return;

            var isRecycled = false;
            FlattenCollectionItemBase? flattenItem = null;
            if (cache.TryGetValue(itemT, out var items))
            {
                var editor = ItemOrListEditor<FlattenCollectionItemBase>.FromRawValue(items);
                flattenItem = editor[editor.Count - 1];
                if (editor.Count == 1)
                    cache.Remove(itemT);
                else
                {
                    editor.Remove(flattenItem);
                    cache[itemT] = editor.GetRawValueInternal();
                }
            }

            if (flattenItem == null)
            {
                var flattenItemInfo = _getNestedCollection(itemT);
                if (flattenItemInfo.IsEmpty)
                    return;

                flattenItem = flattenItemInfo.GetCollectionItem(item, this);
            }

            _collectionItems.AddRaw(index, flattenItem);
            flattenItem.OnAdded(this, decoratorManager, source, index, false, isRecycled, out _);
        }

        void ILockerChangedListener<IReadOnlyObservableCollection>.OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < _collectionItems.Size; i++)
                _collectionItems.Indexes[i].Value.UpdateLocker(locker);
        }
    }
}