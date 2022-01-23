using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class ConvertCollectionDecorator<T, TTo> : CollectionDecoratorBase
        where TTo : class?
    {
        private readonly bool _allowNull;
        private readonly Action<T, TTo>? _cleanup;
        private readonly IEqualityComparer<T>? _comparerFrom;
        private readonly IEqualityComparer<TTo?>? _comparerTo;
        private IndexMapList<(T? from, TTo? to)> _items;
#pragma warning disable 8714
        private Dictionary<T, TTo?>? _resetCache;
#pragma warning restore 8714

        public ConvertCollectionDecorator(int priority, bool allowNull, Func<T, TTo?, Optional<TTo>> converter, Action<T, TTo>? cleanup,
            IEqualityComparer<T>? comparerFrom, IEqualityComparer<TTo?>? comparerTo) : base(priority)
        {
            Should.NotBeNull(converter, nameof(converter));
            _items = IndexMapList<(T?, TTo?)>.Get();
            Converter = converter;
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _cleanup = cleanup;
            _comparerFrom = comparerFrom;
            _comparerTo = comparerTo;
        }

        public Func<T, TTo?, Optional<TTo>> Converter { get; }

        protected override bool HasAdditionalItems => _items.Size != 0;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected override bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (item.TryCastNullable<TTo>(out var toItem))
            {
                for (var i = 0; i < _items.Size; i++)
                {
                    if (_comparerTo.EqualsOrDefault(toItem, _items.Indexes[i].Value.to))
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
            if (!item.TryCast<T>(_allowNull, out var itemT))
                return true;

            var oldIndex = _items.BinarySearch(index);
            if (oldIndex < 0)
            {
                if (TryConvert(itemT, null, out var converted))
                {
                    _items.Add(index, (itemT, converted), oldIndex);
                    decoratorManager.OnReplaced(collection, this, item, converted, index);
                    return false;
                }
            }
            else
            {
                var oldItem = _items.Indexes[oldIndex].Value.to;
                var hasNewItem = TryConvert(itemT, oldItem, out var newItem);
                if (!hasNewItem || !_comparerTo.EqualsOrDefault(oldItem, newItem))
                {
                    _cleanup?.Invoke(itemT!, oldItem!);
                    if (hasNewItem)
                    {
                        _items.Indexes[oldIndex].Value = (itemT, newItem);
                        decoratorManager.OnReplaced(collection, this, oldItem, newItem, index);
                    }
                    else
                    {
                        _items.RemoveAt(oldIndex);
                        decoratorManager.OnReplaced(collection, this, oldItem, item, index);
                    }

                    return false;
                }

                item = oldItem;
            }

            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            item = TryAdd(item, index, null, true);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            int? oldIndex = oldItem.TryCheckCast<T>(_allowNull) ? _items.BinarySearch(index) : null;
            if (oldIndex.GetValueOrDefault(-1) < 0)
                newItem = TryAdd(newItem, index, null, false, oldIndex);
            else
            {
                if (newItem.TryCast<T>(_allowNull, out var newItemT) && TryConvert(newItemT, null, out var value))
                {
                    oldItem = RemoveRaw(oldIndex!.Value, false);
                    _items.Indexes[oldIndex.Value].Value = (newItemT, value);
                    newItem = value;
                }
                else
                    oldItem = RemoveRaw(oldIndex!.Value, true);
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
            if (items.IsNullOrEmpty())
                Clear();
            else
            {
#pragma warning disable 8714
                _resetCache ??= new Dictionary<T, TTo?>(_items.Size, _comparerFrom);
#pragma warning restore 8714
                TTo? nullValue = default;
                var hasNullValue = false;
                for (var i = 0; i < _items.Size; i++)
                {
                    var item = _items.Indexes[i].Value;
                    if (item.from == null)
                    {
                        nullValue = item.to;
                        hasNullValue = true;
                    }
                    else
                        _resetCache[item.from] = item.to;
                }

                _items.Clear();

                var index = 0;
                foreach (var item in items)
                {
                    if (item.TryCast<T>(_allowNull, out var itemT))
                    {
                        bool hasOldValue;
                        TTo? oldValue;
                        if (itemT == null)
                        {
                            hasOldValue = hasNullValue;
                            oldValue = nullValue;
                            nullValue = default;
                            hasNullValue = false;
                        }
                        else
                        {
#pragma warning disable 8714
                            hasOldValue = _resetCache.Remove(itemT, out oldValue);
#pragma warning restore 8714
                        }

                        if (TryConvert(itemT, oldValue, out var value))
                        {
                            _items.AddRaw(index, (itemT, value));
                            if (hasOldValue && !_comparerTo.EqualsOrDefault(oldValue, value))
                                _cleanup?.Invoke(itemT!, oldValue!);
                        }
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

        private object? TryAdd(object? item, int index, TTo? convertItem, bool updateIndexes, int? binarySearchIndex = null)
        {
            if (updateIndexes)
            {
                binarySearchIndex ??= _items.BinarySearch(index);
                _items.UpdateIndexesBinary(binarySearchIndex.Value, 1);
            }

            if (!item.TryCast<T>(_allowNull, out var itemT) || !TryConvert(itemT, convertItem, out var value))
                return item;

            if (binarySearchIndex == null)
                _items.Add(index, (itemT, value));
            else
                _items.Add(index, (itemT, value), binarySearchIndex.Value);

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
            _cleanup?.Invoke(value.from!, value.to!);
            return value.to;
        }

        private void Clear()
        {
            if (_cleanup != null)
            {
                for (var i = 0; i < _items.Size; i++)
                {
                    var item = _items.Indexes[i].Value;
                    _cleanup(item.from!, item.to!);
                }
            }

            _items.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryConvert(T? item, TTo? convertItem, out TTo? convertedResult)
        {
            var r = Converter(item!, convertItem);
            convertedResult = r.GetValueOrDefault();
            return r.HasValue;
        }
    }
}