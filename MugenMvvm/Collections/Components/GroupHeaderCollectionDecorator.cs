using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
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
        private readonly UpdateHeaderDelegate? _updateHeader;
        private readonly Dictionary<object, HeaderInfo> _headers;

        public GroupHeaderCollectionDecorator(Func<object?, object?> getHeader, UpdateHeaderDelegate? updateHeader = null,
            IEqualityComparer<object>? comparer = null, int priority = CollectionComponentPriority.GroupHeaderDecorator)
        {
            Should.NotBeNull(getHeader, nameof(getHeader));
            _getHeader = getHeader;
            _updateHeader = updateHeader;
            _headers = new Dictionary<object, HeaderInfo>(comparer ?? EqualityComparer<object>.Default);
            Priority = priority;
        }

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            DecoratorManager = null;
        }

        private void Clear()
        {
            if (_updateHeader != null)
            {
                foreach (var header in _headers)
                    _updateHeader(header.Key, GroupHeaderChangedAction.Clear, null);
            }

            _headers.Clear();
        }

        private void AddHeaderIfNeed(ICollection collection, object? header, object? item, bool notify = true)
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

            _updateHeader?.Invoke(header, GroupHeaderChangedAction.ItemAdded, item);
            ++info.UsageCount;
        }

        private void RemoveHeaderIfNeed(ICollection collection, object? header, object? item)
        {
            if (header == null)
                return;

            var headerInfo = _headers[header];
            if (--headerInfo.UsageCount != 0)
            {
                _updateHeader?.Invoke(header, GroupHeaderChangedAction.ItemRemoved, item);
                return;
            }

            _headers.Remove(header);
            foreach (var info in _headers)
            {
                if (info.Value.Index > headerInfo.Index)
                    --info.Value.Index;
            }

            DecoratorManager!.OnRemoved(collection, this, header, headerInfo.Index);
            _updateHeader?.Invoke(header, GroupHeaderChangedAction.Clear, item);
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

            if (_updateHeader != null)
            {
                var header = _getHeader(item);
                if (header != null)
                    _updateHeader(header, GroupHeaderChangedAction.ItemChanged, item);
            }

            index += _headers.Count;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            var header = _getHeader(item);
            using var t = BatchIfNeed(collection, header, null);
            AddHeaderIfNeed(collection, header, item);
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
            if (!_headers.Comparer.Equals(oldItemHeader!, newItemHeader!))
            {
                RemoveHeaderIfNeed(collection, oldItemHeader, oldItem);
                AddHeaderIfNeed(collection, newItemHeader, newItem);
            }
            else if (oldItemHeader != null && _updateHeader != null)
            {
                _updateHeader(oldItemHeader, GroupHeaderChangedAction.ItemRemoved, oldItem);
                _updateHeader(oldItemHeader, GroupHeaderChangedAction.ItemAdded, newItem);
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
            RemoveHeaderIfNeed(collection, header, item);
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

            Clear();
            if (items != null)
            {
                foreach (var item in items)
                    AddHeaderIfNeed(collection, _getHeader(item), item, false);
                items = Decorate(items);
            }

            return true;
        }

        public delegate void UpdateHeaderDelegate(object header, GroupHeaderChangedAction action, object? item);

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