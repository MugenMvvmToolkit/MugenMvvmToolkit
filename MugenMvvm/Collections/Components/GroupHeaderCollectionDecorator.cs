using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class GroupHeaderCollectionDecorator : CollectionDecoratorBase
    {
        private readonly Func<object?, object?> _getHeader;
        private readonly UpdateHeaderDelegate? _updateHeader;
        private readonly Dictionary<object, HeaderInfo> _headers;

        public GroupHeaderCollectionDecorator(Func<object?, object?> getHeader, UpdateHeaderDelegate? updateHeader = null,
            IEqualityComparer<object>? comparer = null, int priority = CollectionComponentPriority.GroupHeaderDecorator) : base(priority)

        {
            Should.NotBeNull(getHeader, nameof(getHeader));
            _getHeader = getHeader;
            _updateHeader = updateHeader;
            _headers = new Dictionary<object, HeaderInfo>(comparer ?? EqualityComparer<object>.Default);
            Priority = priority;
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (_updateHeader != null)
            {
                var header = _getHeader(item);
                if (header != null)
                    _updateHeader(header, GroupHeaderChangedAction.ItemChanged, item);
            }

            index += _headers.Count;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var header = _getHeader(item);
            AddHeaderIfNeed(decoratorManager, collection, header, item);
            index += _headers.Count;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var oldItemHeader = _getHeader(oldItem);
            var newItemHeader = _getHeader(newItem);
            if (!_headers.Comparer.Equals(oldItemHeader!, newItemHeader!))
            {
                RemoveHeaderIfNeed(decoratorManager, collection, oldItemHeader, oldItem);
                AddHeaderIfNeed(decoratorManager, collection, newItemHeader, newItem);
            }
            else if (oldItemHeader != null && _updateHeader != null)
            {
                _updateHeader(oldItemHeader, GroupHeaderChangedAction.ItemRemoved, oldItem);
                _updateHeader(oldItemHeader, GroupHeaderChangedAction.ItemAdded, newItem);
            }

            index += _headers.Count;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            oldIndex += _headers.Count;
            newIndex += _headers.Count;
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var header = _getHeader(item);
            RemoveHeaderIfNeed(decoratorManager, collection, header, item);
            index += _headers.Count;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            Clear();
            if (items != null)
            {
                foreach (var item in items)
                    AddHeaderIfNeed(decoratorManager, collection, _getHeader(item), item, false);
                items = Decorate(items);
            }

            return true;
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

        private void AddHeaderIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? header, object? item,
            bool notify = true)
        {
            if (header == null)
                return;

            if (!_headers.TryGetValue(header, out var info))
            {
                info = new HeaderInfo(_headers.Count);
                _headers[header] = info;
                if (notify)
                    decoratorManager.OnAdded(collection, this, header, info.Index);
            }

            ++info.UsageCount;
            _updateHeader?.Invoke(header, GroupHeaderChangedAction.ItemAdded, item);
        }

        private void RemoveHeaderIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, object? header, object? item)
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

            decoratorManager.OnRemoved(collection, this, header, headerInfo.Index);
            _updateHeader?.Invoke(header, GroupHeaderChangedAction.Clear, item);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var header in _headers.OrderBy(pair => pair.Value.Index))
                yield return header.Key;
            foreach (var item in items)
                yield return item;
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