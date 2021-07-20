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
    public class GroupHeaderCollectionDecorator<T, TGroup> : CollectionDecoratorBase where TGroup : class
    {
        private readonly Func<T, TGroup?> _getGroup;
        private readonly UpdateGroupDelegate? _updateGroup;
        private readonly Dictionary<TGroup, GroupInfo> _groups;

        public GroupHeaderCollectionDecorator(Func<T, TGroup?> getGroup, UpdateGroupDelegate? updateGroup = null,
            IEqualityComparer<TGroup>? comparer = null, int priority = CollectionComponentPriority.GroupHeaderDecorator) : base(priority)
        {
            Should.NotBeNull(getGroup, nameof(getGroup));
            _getGroup = getGroup;
            _updateGroup = updateGroup;
            _groups = new Dictionary<TGroup, GroupInfo>(comparer ?? EqualityComparer<TGroup>.Default);
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
            if (item is T t)
            {
                if (_updateGroup != null)
                {
                    var group = _getGroup(t);
                    if (group != null)
                        _updateGroup(group, GroupHeaderChangedAction.ItemChanged, t, args);
                }
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (item is T t)
            {
                var group = _getGroup(t);
                AddGroupIfNeed(decoratorManager, collection, group, t);
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var oldItemGroup = GetGroup(oldItem);
            var newItemGroup = GetGroup(newItem);
            if (!_groups.Comparer.Equals(oldItemGroup!, newItemGroup!))
            {
                RemoveGroupIfNeed(decoratorManager, collection, oldItemGroup, oldItemGroup == null ? default! : (T) oldItem!);
                AddGroupIfNeed(decoratorManager, collection, newItemGroup, newItemGroup == null ? default! : (T) newItem!);
            }
            else if (oldItemGroup != null && _updateGroup != null)
            {
                _updateGroup(oldItemGroup, GroupHeaderChangedAction.ItemRemoved, (T) oldItem!, null);
                _updateGroup(oldItemGroup, GroupHeaderChangedAction.ItemAdded, (T) newItem!, null);
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            oldIndex += _groups.Count;
            newIndex += _groups.Count;
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (item is T t)
            {
                var group = _getGroup(t);
                RemoveGroupIfNeed(decoratorManager, collection, group, t);
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item is T t)
                        AddGroupIfNeed(decoratorManager, collection, _getGroup(t), t, false);
                }

                items = Decorate(items);
            }

            return true;
        }

        private TGroup? GetGroup(object? item) => item is T t ? _getGroup(t) : null;

        private void Clear()
        {
            if (_updateGroup != null)
            {
                foreach (var group in _groups)
                    _updateGroup(group.Key, GroupHeaderChangedAction.Clear, default!, null);
            }

            _groups.Clear();
        }

        private void AddGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TGroup? group, T item, bool notify = true)
        {
            if (group == null)
                return;

            if (!_groups.TryGetValue(group, out var info))
            {
                info = new GroupInfo(_groups.Count);
                _groups[group] = info;
                if (notify)
                    decoratorManager.OnAdded(collection, this, group, info.Index);
            }

            ++info.UsageCount;
            _updateGroup?.Invoke(group, GroupHeaderChangedAction.ItemAdded, item, null);
        }

        private void RemoveGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TGroup? group, T item)
        {
            if (group == null)
                return;

            var groupInfo = _groups[group];
            if (--groupInfo.UsageCount != 0)
            {
                _updateGroup?.Invoke(group, GroupHeaderChangedAction.ItemRemoved, item, null);
                return;
            }

            _groups.Remove(group);
            foreach (var info in _groups)
            {
                if (info.Value.Index > groupInfo.Index)
                    --info.Value.Index;
            }

            decoratorManager.OnRemoved(collection, this, group, groupInfo.Index);
            _updateGroup?.Invoke(group, GroupHeaderChangedAction.Clear, item, null);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var group in _groups.OrderBy(pair => pair.Value.Index))
                yield return group.Key;
            foreach (var item in items)
                yield return item;
        }

        public delegate void UpdateGroupDelegate(TGroup group, GroupHeaderChangedAction action, T item, object? args);

        private sealed class GroupInfo
        {
            public int UsageCount;
            public int Index;

            public GroupInfo(int index)
            {
                Index = index;
            }
        }
    }
}