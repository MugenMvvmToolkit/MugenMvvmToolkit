using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class ItemHeaderFooterCollectionDecorator : CollectionDecoratorBase, IComparer<ItemHeaderFooterCollectionDecorator.ItemInfo>
    {
        private readonly Func<object?, bool?> _isHeaderOrFooter;
        private readonly IComparer<object?>? _headerComparer;
        private readonly IComparer<object?>? _footerComparer;
        private readonly ListInternal<ItemInfo> _headers;
        private readonly ListInternal<ItemInfo> _footers;
        private IComparer<object?>? _currentComparer;
        private int _footerIndex;

        public ItemHeaderFooterCollectionDecorator(Func<object?, bool?> isHeaderOrFooter, IComparer<object?>? headerComparer = null, IComparer<object?>? footerComparer = null,
            int priority = CollectionComponentPriority.HeaderFooterDecorator) : base(priority)
        {
            _isHeaderOrFooter = isHeaderOrFooter;
            _headerComparer = headerComparer;
            _footerComparer = footerComparer;
            _headers = new ListInternal<ItemInfo>(0);
            _footers = new ListInternal<ItemInfo>(0);
            Priority = priority;
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _headers.Clear();
            _footers.Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            var isHeaderOrFooter = _isHeaderOrFooter(item);
            if (isHeaderOrFooter != null)
            {
                if (isHeaderOrFooter.Value)
                {
                    index = _headers.IndexOf(new ItemInfo(item, index));
                    return index >= 0;
                }

                index = _footers.IndexOf(new ItemInfo(item, index));
                if (index < 0)
                    return false;

                index += _footerIndex;
                return true;
            }

            index = GetIndex(index);
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            AddHeaderOrFooter(item, ref index);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var oldItemIsHeaderOrFooter = _isHeaderOrFooter(oldItem);
            var newItemIsHeaderOrFooter = _isHeaderOrFooter(newItem);
            if (oldItemIsHeaderOrFooter == null && newItemIsHeaderOrFooter == null)
            {
                index = GetIndex(index);
                return true;
            }

            var removeIndex = index;
            RemoveHeaderOrFooter(oldItem, ref removeIndex, oldItemIsHeaderOrFooter, true);
            AddHeaderOrFooter(newItem, ref index, newItemIsHeaderOrFooter, true);

            if (removeIndex == index)
                return true;

            decoratorManager.OnRemoved(collection, this, oldItem, removeIndex);
            decoratorManager.OnAdded(collection, this, newItem, index);

            return false;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            RemoveHeaderOrFooter(item, ref oldIndex);
            AddHeaderOrFooter(item, ref newIndex);
            return oldIndex != newIndex;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            RemoveHeaderOrFooter(item, ref index);
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            _headers.Clear();
            _footers.Clear();
            var count = 0;
            if (items != null)
            {
                var i = 0;
                foreach (var item in items)
                {
                    var index = i;
                    if (!AddHeaderOrFooter(item, ref index, ignoreIndexes: true))
                        ++count;
                    ++i;
                }
            }

            _footerIndex = count + _headers.Count;
            items = Decorate(items);
            return true;
        }

        private static void UpdateIndexes(ListInternal<ItemInfo> items, int index, int value)
        {
            var count = items.Count;
            var array = items.Items;
            for (var i = 0; i < count; i++)
            {
                if (array[i].OriginalIndex >= index)
                    array[i].OriginalIndex += value;
            }
        }

        private int Add(ListInternal<ItemInfo> items, object? item, int index, IComparer<object?>? comparer)
        {
            _currentComparer = comparer;
            return items.AddOrdered(new ItemInfo(item, index), this);
        }

        private bool AddHeaderOrFooter(object? item, ref int index, bool? isHeaderOrFooter = null, bool hasValue = false, bool ignoreIndexes = false)
        {
            if (!ignoreIndexes)
            {
                UpdateIndexes(_headers, index, 1);
                UpdateIndexes(_footers, index, 1);
            }

            if (!hasValue)
                isHeaderOrFooter = _isHeaderOrFooter(item);
            if (isHeaderOrFooter == null)
            {
                if (!ignoreIndexes)
                {
                    index = GetIndex(index);
                    if (index > _footerIndex)
                        --index;
                    ++_footerIndex;
                }

                return false;
            }

            if (isHeaderOrFooter.Value)
            {
                index = Add(_headers, item, index, _headerComparer);
                ++_footerIndex;
            }
            else
                index = Add(_footers, item, index, _footerComparer) + _footerIndex;

            return true;
        }

        private void RemoveHeaderOrFooter(object? item, ref int index, bool? isHeaderOrFooter = null, bool hasValue = false)
        {
            var originalIndex = index;
            if (!hasValue)
                isHeaderOrFooter = _isHeaderOrFooter(item);
            if (isHeaderOrFooter == null)
            {
                index = GetIndex(index);
                --_footerIndex;
            }
            else if (isHeaderOrFooter.Value)
            {
                index = _headers.IndexOf(new ItemInfo(item, index));
                if (index < 0)
                    ExceptionManager.ThrowNotValidArgument(nameof(item));
                _headers.RemoveAt(index);
                --_footerIndex;
            }
            else
            {
                index = _footers.IndexOf(new ItemInfo(item, index));
                if (index < 0)
                    ExceptionManager.ThrowNotValidArgument(nameof(item));
                _footers.RemoveAt(index);
                index = index + _footerIndex;
            }

            UpdateIndexes(_headers, originalIndex, -1);
            UpdateIndexes(_footers, originalIndex, -1);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?>? items)
        {
            var count = _headers.Count;
            var array = _headers.Items;
            for (var index = 0; index < count; index++)
                yield return array[index].Item;

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (_isHeaderOrFooter(item) == null)
                        yield return item;
                }
            }

            count = _footers.Count;
            array = _footers.Items;
            for (var index = 0; index < count; index++)
                yield return array[index].Item;
        }

        private int GetIndex(int index)
        {
            var result = index;
            var count = _headers.Count;
            var items = _headers.Items;
            for (var i = 0; i < count; i++)
            {
                if (items[i].OriginalIndex >= index)
                    ++result;
            }

            count = _footers.Count;
            items = _footers.Items;
            for (var i = 0; i < count; i++)
            {
                if (items[i].OriginalIndex < index)
                    --result;
            }

            return result;
        }

        int IComparer<ItemInfo>.Compare(ItemInfo x, ItemInfo y)
        {
            if (_currentComparer != null)
            {
                var r = _currentComparer.Compare(x.Item, y.Item);
                if (r != 0)
                    return r;
            }

            return x.OriginalIndex.CompareTo(y.OriginalIndex);
        }

        [StructLayout(LayoutKind.Auto)]
        private struct ItemInfo : IEquatable<ItemInfo>
        {
            public readonly object? Item;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            public int OriginalIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemInfo(object? item, int originalIndex)
            {
                Item = item;
                OriginalIndex = originalIndex;
            }

            public bool Equals(ItemInfo other) => Equals(Item, other.Item) && OriginalIndex == other.OriginalIndex;
        }
    }
}