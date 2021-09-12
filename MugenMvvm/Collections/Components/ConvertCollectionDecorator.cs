using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class ConvertCollectionDecorator<T, TTo> : CollectionDecoratorBase
        where T : notnull
        where TTo : class?
    {
        private readonly Action<T, TTo>? _cleanup;
        private readonly IEqualityComparer<TTo?> _comparer;
        private IndexMapList<(T from, TTo? to)> _items;
        private Dictionary<T, TTo?>? _resetCache;

        public ConvertCollectionDecorator(Func<T, TTo?, TTo?> converter, Action<T, TTo>? cleanup = null, IEqualityComparer<TTo?>? comparer = null,
            int priority = CollectionComponentPriority.ConverterDecorator) : base(priority)
        {
            Should.NotBeNull(converter, nameof(converter));
            _items = IndexMapList<(T, TTo?)>.Get();
            Converter = converter;
            _cleanup = cleanup;
            _comparer = comparer ?? EqualityComparer<TTo?>.Default;
        }

        public Func<T, TTo?, TTo?> Converter { get; }

        protected override bool HasAdditionalItems => _items.Size != 0;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected override bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (item is TTo toItem)
            {
                for (var i = 0; i < _items.Size; i++)
                {
                    if (_comparer.Equals(toItem, _items.Indexes[i].Value.to))
                    {
                        indexes.Add(_items.Indexes[i].Index);
                        if (ignoreDuplicates)
                            return true;
                    }
                }
            }

            return true;
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items)
        {
            if (_items.Size == 0)
                return items;
            return DecorateImpl(items);
        }

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (item is not T itemT)
                return true;

            var oldIndex = _items.BinarySearch(index);
            var oldItem = _items.Indexes[oldIndex].Value.to;
            var newItem = Converter(itemT, oldItem);
            if (!_comparer.Equals(oldItem, newItem))
            {
                _cleanup?.Invoke(itemT, oldItem!);
                _items.Indexes[oldIndex].Value = (itemT, newItem);
                decoratorManager.OnReplaced(collection, this, oldItem, newItem, index);
                return false;
            }

            item = oldItem;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = TryAdd(item, index, null, true, false);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var oldIndex = oldItem is T ? _items.BinarySearch(index) : -1;
            if (oldIndex == -1)
                newItem = TryAdd(newItem, index, null, false, false);
            else
            {
                if (newItem is T newItemT)
                {
                    oldItem = RemoveRaw(oldIndex, false);
                    var value = Converter(newItemT, null);
                    _items.Indexes[oldIndex].Value = (newItemT, value);
                    newItem = value;
                }
                else
                    oldItem = RemoveRaw(oldIndex, true);
            }

            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (_items.Move(oldIndex, newIndex, out var value))
                item = value.to;
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = TryRemove(item, index);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items == null)
                Clear();
            else
            {
                _resetCache ??= new Dictionary<T, TTo?>(_items.Size, GetComparer());
                for (var i = 0; i < _items.Size; i++)
                {
                    var item = _items.Indexes[i].Value;
                    _resetCache[item.from] = item.to;
                }

                _items.Clear();

                var index = 0;
                foreach (var item in items)
                {
                    if (item is T itemT)
                    {
                        _resetCache.Remove(itemT, out var oldValue);
                        var added = TryAdd(item, index, oldValue, false, true);
                        if (oldValue != null && !_comparer.Equals(oldValue, added as TTo))
                            _cleanup?.Invoke(itemT, oldValue);
                    }

                    ++index;
                }

                if (_cleanup != null)
                {
                    foreach (var oldValue in _resetCache)
                        _cleanup.Invoke(oldValue.Key, oldValue.Value!);
                }

                _resetCache.Clear();
                items = Decorate(decoratorManager, collection, items);
            }

            return true;
        }

        private static IEqualityComparer<T> GetComparer() => typeof(T).IsValueType ? EqualityComparer<T>.Default : (IEqualityComparer<T>) InternalEqualityComparer.Reference;

        private IEnumerable<object?> DecorateImpl(IEnumerable<object?> items)
        {
            var index = 0;
            var itemIndex = 0;
            foreach (var item in items)
            {
                if (itemIndex < _items.Size)
                {
                    var entry = _items.Indexes[itemIndex];
                    if (entry.Index == index)
                    {
                        ++index;
                        ++itemIndex;
                        yield return entry.Value.to;
                        continue;
                    }
                }

                ++index;
                yield return item;
            }
        }

        private object? TryAdd(object? item, int index, TTo? convertItem, bool updateIndexes, bool addRaw)
        {
            int binarySearchIndex;
            if (updateIndexes)
            {
                binarySearchIndex = _items.BinarySearch(index);
                _items.UpdateIndexesBinary(binarySearchIndex, 1);
            }
            else
                binarySearchIndex = -1;

            if (item is not T itemT)
                return item;

            var value = Converter(itemT, convertItem);
            if (addRaw)
                _items.AddRaw(index, (itemT, value));
            else
            {
                if (updateIndexes)
                    _items.Add(index, (itemT, value), binarySearchIndex);
                else
                    _items.Add(index, (itemT, value));
            }

            return value;
        }

        private object? TryRemove(object? item, int index)
        {
            var indexToRemove = _items.BinarySearch(index);
            _items.UpdateIndexesBinary(indexToRemove, -1);
            if (indexToRemove < 0)
                return item;
            return RemoveRaw(indexToRemove, true);
        }

        private object? RemoveRaw(int index, bool removeFromCollection)
        {
            var value = _items.Indexes[index].Value;
            if (removeFromCollection)
                _items.RemoveAt(index);
            _cleanup?.Invoke(value.from, value.to!);
            return value.to;
        }

        private void Clear()
        {
            if (_cleanup != null)
            {
                for (var i = 0; i < _items.Size; i++)
                {
                    var item = _items.Indexes[i].Value;
                    _cleanup(item.from, item.to!);
                }
            }

            _items.Clear();
        }
    }
}