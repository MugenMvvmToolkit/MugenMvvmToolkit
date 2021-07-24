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
    public class ItemHeaderFooterCollectionDecorator<T> : CollectionDecoratorBase, IComparer<ItemHeaderFooterCollectionDecorator<T>.ItemInfo>
    {
        private readonly Func<T, bool?> _isHeaderOrFooter;
        private readonly IComparer<T>? _headerComparer;
        private readonly IComparer<T>? _footerComparer;
        private ListInternal<ItemInfo> _headers;
        private ListInternal<ItemInfo> _footers;
        private IComparer<T>? _currentComparer;
        private int _footerIndex;

        public ItemHeaderFooterCollectionDecorator(Func<T, bool?> isHeaderOrFooter, IComparer<T>? headerComparer = null, IComparer<T>? footerComparer = null,
            int priority = CollectionComponentPriority.HeaderFooterDecorator) : base(priority)
        {
            Should.NotBeNull(isHeaderOrFooter, nameof(isHeaderOrFooter));
            _isHeaderOrFooter = isHeaderOrFooter;
            _headerComparer = headerComparer;
            _footerComparer = footerComparer;
            _headers = new ListInternal<ItemInfo>(0);
            _footers = new ListInternal<ItemInfo>(0);
            Priority = priority;
        }

        public override bool HasAdditionalItems => false;

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
            if (item is T itemT)
            {
                bool? isHeaderOrFooterOld = null;
                var headerIndex = _headers.IndexOf(new ItemInfo(itemT, index));
                int footerIndex;
                if (headerIndex < 0)
                {
                    footerIndex = _footers.IndexOf(new ItemInfo(itemT, index));
                    if (footerIndex >= 0)
                        isHeaderOrFooterOld = false;
                }
                else
                {
                    footerIndex = -1;
                    isHeaderOrFooterOld = true;
                }

                var isHeaderOrFooterNew = _isHeaderOrFooter(itemT);
                if (isHeaderOrFooterOld != isHeaderOrFooterNew)
                    return Replace(decoratorManager, collection, isHeaderOrFooterOld, isHeaderOrFooterNew, item, item, ref index, headerIndex, footerIndex);

                if (isHeaderOrFooterNew != null)
                {
                    if (isHeaderOrFooterNew.Value)
                    {
                        index = headerIndex;
                        return headerIndex >= 0;
                    }

                    if (footerIndex < 0)
                        return false;

                    index = footerIndex + _footerIndex;
                    return true;
                }
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
            var isHeaderOrFooterOld = oldItem is T oldT ? _isHeaderOrFooter(oldT) : null;
            var isHeaderOrFooterNew = newItem is T newT ? _isHeaderOrFooter(newT) : null;
            if (isHeaderOrFooterOld == null && isHeaderOrFooterNew == null)
            {
                index = GetIndex(index);
                return true;
            }

            return Replace(decoratorManager, collection, isHeaderOrFooterOld, isHeaderOrFooterNew, oldItem, newItem, ref index);
        }

        private bool Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            bool? isHeaderOrFooterOld, bool? isHeaderOrFooterNew, object? oldItem, object? newItem, ref int index, int? headerIndex = null, int? footerIndex = null)
        {
            var removeIndex = index;
            RemoveHeaderOrFooter(oldItem, ref removeIndex, isHeaderOrFooterOld, true, headerIndex, footerIndex);
            AddHeaderOrFooter(newItem, ref index, isHeaderOrFooterNew, true);

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

        private static void UpdateIndexes(ref ListInternal<ItemInfo> items, int index, int value)
        {
            var count = items.Count;
            var array = items.Items;
            for (var i = 0; i < count; i++)
            {
                if (array[i].OriginalIndex >= index)
                    array[i].OriginalIndex += value;
            }
        }

        private int Add(ref ListInternal<ItemInfo> items, T item, int index, IComparer<T>? comparer)
        {
            _currentComparer = comparer;
            return items.AddOrdered(new ItemInfo(item, index), this);
        }

        private bool AddHeaderOrFooter(object? item, ref int index, bool? isHeaderOrFooter = null, bool hasValue = false, bool ignoreIndexes = false)
        {
            if (!ignoreIndexes)
            {
                UpdateIndexes(ref _headers, index, 1);
                UpdateIndexes(ref _footers, index, 1);
            }

            if (!hasValue)
                isHeaderOrFooter = item is T t ? _isHeaderOrFooter(t) : null;
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
                index = Add(ref _headers, (T) item!, index, _headerComparer);
                ++_footerIndex;
            }
            else
                index = Add(ref _footers, (T) item!, index, _footerComparer) + _footerIndex;

            return true;
        }

        private void RemoveHeaderOrFooter(object? item, ref int index, bool? isHeaderOrFooter = null, bool hasValue = false, int? headerIndex = null, int? footerIndex = null)
        {
            var originalIndex = index;
            if (!hasValue)
                isHeaderOrFooter = item is T t ? _isHeaderOrFooter(t) : null;
            if (isHeaderOrFooter == null)
            {
                index = GetIndex(index);
                --_footerIndex;
            }
            else if (isHeaderOrFooter.Value)
            {
                index = headerIndex ?? _headers.IndexOf(new ItemInfo((T) item!, index));
                if (index < 0)
                    ExceptionManager.ThrowNotValidArgument(nameof(item));
                _headers.RemoveAt(index);
                --_footerIndex;
            }
            else
            {
                index = footerIndex ?? _footers.IndexOf(new ItemInfo((T) item!, index));
                if (index < 0)
                    ExceptionManager.ThrowNotValidArgument(nameof(item));
                _footers.RemoveAt(index);
                index = index + _footerIndex;
            }

            UpdateIndexes(ref _headers, originalIndex, -1);
            UpdateIndexes(ref _footers, originalIndex, -1);
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
                    if (item is not T t || _isHeaderOrFooter(t) == null)
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
            public readonly T Item;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            public int OriginalIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ItemInfo(T item, int originalIndex)
            {
                Item = item;
                OriginalIndex = originalIndex;
            }

            public bool Equals(ItemInfo other) => EqualityComparer<T>.Default.Equals(Item, other.Item) && OriginalIndex == other.OriginalIndex;
        }
    }
}