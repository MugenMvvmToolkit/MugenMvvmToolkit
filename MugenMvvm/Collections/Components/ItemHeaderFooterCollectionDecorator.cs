using System;
using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class ItemHeaderFooterCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority, IComparer<(object?, int)>
    {
        private readonly Func<object?, bool?> _isHeaderOrFooter;
        private readonly IComparer<object?>? _headerComparer;
        private readonly IComparer<object?>? _footerComparer;
        private readonly List<(object? item, int originalIndex)> _headers;
        private readonly List<(object? item, int originalIndex)> _footers;
        private IComparer<object?>? _currentComparer;
        private ICollectionDecoratorManagerComponent? _decoratorManager;
        private int _footerIndex;

        public ItemHeaderFooterCollectionDecorator(Func<object?, bool?> isHeaderOrFooter, IComparer<object?>? headerComparer = null, IComparer<object?>? footerComparer = null,
            int priority = CollectionComponentPriority.HeaderFooterDecorator)
        {
            _isHeaderOrFooter = isHeaderOrFooter;
            _headerComparer = headerComparer;
            _footerComparer = footerComparer;
            _headers = new List<(object? item, int originalIndex)>();
            _footers = new List<(object? item, int originalIndex)>();
            Priority = priority;
        }

        public int Priority { get; set; }

        private static void UpdateIndexes(List<(object? item, int originalIndex)> items, int index, int value)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var header = items[i];
                if (header.originalIndex >= index)
                    items[i] = (header.item, header.originalIndex + value);
            }
        }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = null;
            _headers.Clear();
            _footers.Clear();
        }

        private int Add(List<(object? item, int originalIndex)> items, object? item, int index, IComparer<object?>? comparer)
        {
            _currentComparer = comparer;
            return MugenExtensions.AddOrdered(items, (item, index), this);
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
                index = _headers.IndexOf((item, index));
                if (index < 0)
                    ExceptionManager.ThrowNotValidArgument(nameof(item));
                _headers.RemoveAt(index);
                --_footerIndex;
            }
            else
            {
                index = _footers.IndexOf((item, index));
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
                yield return _headers[index].item;

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (_isHeaderOrFooter(item) == null)
                        yield return item;
                }
            }

            for (var index = 0; index < _footers.Count; index++)
                yield return _footers[index].item;
        }

        private int GetIndex(int index)
        {
            var result = index;
            for (var i = 0; i < _headers.Count; i++)
            {
                if (_headers[i].originalIndex >= index)
                    ++result;
            }

            for (var i = 0; i < _footers.Count; i++)
            {
                if (_footers[i].originalIndex < index)
                    --result;
            }

            return result;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => _decoratorManager == null ? items : Decorate(items);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (_decoratorManager == null)
                return false;

            var isHeaderOrFooter = _isHeaderOrFooter(item);
            if (isHeaderOrFooter != null)
            {
                if (isHeaderOrFooter.Value)
                {
                    index = _headers.IndexOf((item, index));
                    return index >= 0;
                }

                index = _footers.IndexOf((item, index));
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
            if (_decoratorManager == null)
                return false;

            AddHeaderOrFooter(item, ref index);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (_decoratorManager == null)
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

            _decoratorManager.OnRemoved(collection, this, oldItem, removeIndex);
            _decoratorManager.OnAdded(collection, this, newItem, index);

            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (_decoratorManager == null)
                return false;

            RemoveHeaderOrFooter(item, ref oldIndex);
            AddHeaderOrFooter(item, ref newIndex);
            return oldIndex != newIndex;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (_decoratorManager == null)
                return false;

            RemoveHeaderOrFooter(item, ref index);
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (_decoratorManager == null)
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

        int IComparer<(object?, int)>.Compare((object?, int) x, (object?, int) y) =>
            _currentComparer == null ? x.Item2.CompareTo(y.Item2) : _currentComparer!.Compare(x.Item1, y.Item1);
    }
}