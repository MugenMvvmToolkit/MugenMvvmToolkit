using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public class GroupHeaderCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private readonly Func<object?, object?> _getHeader;
        private readonly Dictionary<object, HeaderInfo> _headers;
        private ICollectionDecoratorManagerComponent? _decoratorManager;

        public GroupHeaderCollectionDecorator(Func<object?, object?> getHeader, int priority = CollectionComponentPriority.GroupHeaderDecorator)
        {
            Should.NotBeNull(getHeader, nameof(getHeader));
            _getHeader = getHeader;
            _headers = new Dictionary<object, HeaderInfo>();
            Priority = priority;
        }

        public int Priority { get; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata) => _decoratorManager = null;

        private void AddHeaderIfNeed(ICollection collection, object? item, bool notify = true)
        {
            var header = _getHeader(item);
            if (header == null)
                return;

            if (!_headers.TryGetValue(header, out var info))
            {
                info = new HeaderInfo(_headers.Count);
                if (notify)
                    _decoratorManager!.OnAdded(collection, this, header, info.Index);
                _headers[header] = info;
            }

            ++info.UsageCount;
        }

        private void RemoveHeaderIfNeed(ICollection collection, object? item)
        {
            var header = _getHeader(item);
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

            _decoratorManager!.OnRemoved(collection, this, header, headerInfo.Index);
        }

        private IEnumerable<object?> EnumerateWithHeaders(IEnumerable<object?> items)
        {
            foreach (var header in _headers.OrderBy(pair => pair.Value.Index))
                yield return header.Key;
            foreach (var item in items)
                yield return item;
        }

        IEnumerable<object?> ICollectionDecorator.DecorateItems(ICollection collection, IEnumerable<object?> items) => EnumerateWithHeaders(items);

        bool ICollectionDecorator.OnItemChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            index += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            AddHeaderIfNeed(collection, item);
            index += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (!ReferenceEquals(_getHeader(oldItem), _getHeader(newItem)))
            {
                RemoveHeaderIfNeed(collection, oldItem);
                AddHeaderIfNeed(collection, newItem);
            }

            index += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            oldIndex += _headers.Count;
            newIndex += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            RemoveHeaderIfNeed(collection, item);
            index += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            _headers.Clear();
            if (items != null)
            {
                foreach (var item in items)
                    AddHeaderIfNeed(collection, item, false);
                items = EnumerateWithHeaders(items);
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