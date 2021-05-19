using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class ItemHeaderFooterCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority,
        IComparer<ItemHeaderFooterCollectionDecorator.ItemInfo>
    {
        private readonly Func<object?, bool?> _isHeaderOrFooter;
        private readonly IComparer<object?>? _headerComparer;
        private readonly IComparer<object?>? _footerComparer;
        private readonly List<ItemInfo> _headers;
        private readonly List<ItemInfo> _footers;
        private IComparer<object?>? _currentComparer;
        private int _footerIndex;

        public ItemHeaderFooterCollectionDecorator(Func<object?, bool?> isHeaderOrFooter, IComparer<object?>? headerComparer = null, IComparer<object?>? footerComparer = null,
            int priority = CollectionComponentPriority.HeaderFooterDecorator)
        {
            _isHeaderOrFooter = isHeaderOrFooter;
            _headerComparer = headerComparer;
            _footerComparer = footerComparer;
            _headers = new List<ItemInfo>();
            _footers = new List<ItemInfo>();
            Priority = priority;
        }

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        private static void UpdateIndexes(List<ItemInfo> items, int index, int value)
        {
#if NET5_0
            var span = CollectionsMarshal.AsSpan(items);
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i].OriginalIndex >= index)
                    span[i].OriginalIndex += value;
            }
#else
            for (var i = 0; i < items.Count; i++)
            {
                var header = items[i];
                if (header.OriginalIndex >= index)
                    items[i] = new ItemInfo(header.Item, header.OriginalIndex + value);
            }
#endif
        }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            DecoratorManager = null;
            _headers.Clear();
            _footers.Clear();
        }

        private int Add(List<ItemInfo> items, object? item, int index, IComparer<object?>? comparer)
        {
            _currentComparer = comparer;
            return MugenExtensions.AddOrdered(items, new ItemInfo(item, index), this);
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
            for (var index = 0; index < _headers.Count; index++)
                yield return _headers[index].Item;

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (_isHeaderOrFooter(item) == null)
                        yield return item;
                }
            }

            for (var index = 0; index < _footers.Count; index++)
                yield return _footers[index].Item;
        }

        private int GetIndex(int index)
        {
            var result = index;
#if NET5_0
            var items = CollectionsMarshal.AsSpan(_headers);
            for (var i = 0; i < items.Length; i++)
#else
            var items = _headers;
            for (var i = 0; i < items.Count; i++)
#endif
            {
                if (items[i].OriginalIndex >= index)
                    ++result;
            }

#if NET5_0
            items = CollectionsMarshal.AsSpan(_footers);
            for (var i = 0; i < items.Length; i++)
#else
            items = _footers;
            for (var i = 0; i < items.Count; i++)
#endif
            {
                if (items[i].OriginalIndex < index)
                    --result;
            }

            return result;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => DecoratorManager == null ? items : Decorate(items);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (DecoratorManager == null)
                return false;

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

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            AddHeaderOrFooter(item, ref index);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (DecoratorManager == null)
                return false;

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

            using var _ = DecoratorManager.BatchUpdate(collection, this);
            DecoratorManager.OnRemoved(collection, this, oldItem, removeIndex);
            DecoratorManager.OnAdded(collection, this, newItem, index);

            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (DecoratorManager == null)
                return false;

            RemoveHeaderOrFooter(item, ref oldIndex);
            AddHeaderOrFooter(item, ref newIndex);
            return oldIndex != newIndex;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            RemoveHeaderOrFooter(item, ref index);
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return false;

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

        int IComparer<ItemInfo>.Compare(ItemInfo x, ItemInfo y) =>
            _currentComparer == null ? x.OriginalIndex.CompareTo(y.OriginalIndex) : _currentComparer!.Compare(x.Item, y.Item);

        [StructLayout(LayoutKind.Auto)]
        private struct ItemInfo : IEquatable<ItemInfo>
        {
            public readonly object? Item;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            public int OriginalIndex;

            public ItemInfo(object? item, int originalIndex)
            {
                Item = item;
                OriginalIndex = originalIndex;
            }

            public bool Equals(ItemInfo other) => Equals(Item, other.Item) && OriginalIndex == other.OriginalIndex;

            public override bool Equals(object? obj) => obj is ItemInfo other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Item);
        }
    }
}