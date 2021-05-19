using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class GroupHeaderCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private readonly Func<object?, object?> _getHeader;
        private readonly Dictionary<object, HeaderInfo> _headers;

        public GroupHeaderCollectionDecorator(Func<object?, object?> getHeader, int priority = CollectionComponentPriority.GroupHeaderDecorator)
        {
            Should.NotBeNull(getHeader, nameof(getHeader));
            _getHeader = getHeader;
            _headers = new Dictionary<object, HeaderInfo>();
            Priority = priority;
        }

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _headers.Clear();
            DecoratorManager = null;
        }

        private void AddHeaderIfNeed1(ICollection collection, object? header, bool notify = true)
        {
            if (header == null)
                return;

            if (!_headers.TryGetValue(header, out var info))
            {
                info = new HeaderInfo(_headers.Count);
                _headers[header] = info;
                if (notify)
                    DecoratorManager!.OnAdded(collection, this, header, info.Index);
            }

            ++info.UsageCount;
        }

        private void RemoveHeaderIfNeed1(ICollection collection, object? header)
        {
            if (header == null)
                return;

            var headerInfo = _headers[header];
            if (--headerInfo.UsageCount != 0)
                return;

            _headers.Remove(header);
            foreach (var info in _headers)
            {
                if (info.Value.Index > headerInfo.Index)
                    --info.Value.Index;
            }

            DecoratorManager!.OnRemoved(collection, this, header, headerInfo.Index);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var header in _headers.OrderBy(pair => pair.Value.Index))
                yield return header.Key;
            foreach (var item in items)
                yield return item;
        }

        private ActionToken BatchIfNeed(ICollection collection, object? addHeader, object? removeHeader)
        {
            if (addHeader != null && !_headers.ContainsKey(addHeader) || removeHeader != null && _headers.TryGetValue(removeHeader, out var info) && info.UsageCount == 1)
                return DecoratorManager!.BatchUpdate(collection, this);
            return default;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => DecoratorManager == null ? items : Decorate(items);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (DecoratorManager == null)
                return false;

            index += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            var header = _getHeader(item);
            using var t = BatchIfNeed(collection, header, null);
            AddHeaderIfNeed1(collection, header);
            index += _headers.Count;
            if (t.IsEmpty)
                return true;

            DecoratorManager.OnAdded(collection, this, item, index);
            return false;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            var oldItemHeader = _getHeader(oldItem);
            var newItemHeader = _getHeader(newItem);
            using var t = BatchIfNeed(collection, newItemHeader, oldItemHeader);
            if (!ReferenceEquals(oldItemHeader, newItemHeader))
            {
                RemoveHeaderIfNeed1(collection, oldItemHeader);
                AddHeaderIfNeed1(collection, newItemHeader);
            }

            index += _headers.Count;
            if (t.IsEmpty)
                return true;

            DecoratorManager.OnReplaced(collection, this, oldItem, newItem, index);
            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (DecoratorManager == null)
                return false;

            oldIndex += _headers.Count;
            newIndex += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            var header = _getHeader(item);
            using var t = BatchIfNeed(collection, null, header);
            RemoveHeaderIfNeed1(collection, header);
            index += _headers.Count;
            if (t.IsEmpty)
                return true;

            DecoratorManager.OnRemoved(collection, this, item, index);
            return false;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return false;

            _headers.Clear();
            if (items != null)
            {
                foreach (var item in items)
                    AddHeaderIfNeed1(collection, _getHeader(item), false);
                items = Decorate(items);
            }

            return true;
        }

        private sealed class HeaderInfo
        {
            public int UsageCount;
            public int Index;

            public HeaderInfo(int index)
            {
                Index = index;
            }
        }
    }
}