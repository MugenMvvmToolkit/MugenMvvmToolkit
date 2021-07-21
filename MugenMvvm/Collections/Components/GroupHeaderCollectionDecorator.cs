using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class GroupHeaderCollectionDecorator<T, TGroup> : CollectionDecoratorBase where TGroup : class
    {
        private readonly Func<T, TGroup?> _getGroup;
        private readonly UpdateGroupDelegate? _updateGroup;
        private readonly Dictionary<TGroup, GroupInfo> _groups;
        private readonly Dictionary<T, ItemGroupInfo>? _itemToGroup;

        public GroupHeaderCollectionDecorator(Func<T, TGroup?> getGroup, UpdateGroupDelegate? updateGroup = null,
            IEqualityComparer<TGroup>? comparer = null, bool hasStableKeys = true, int priority = CollectionComponentPriority.GroupHeaderDecorator) : base(priority)
        {
            Should.NotBeNull(getGroup, nameof(getGroup));
            _getGroup = getGroup;
            _updateGroup = updateGroup;
            _groups = new Dictionary<TGroup, GroupInfo>(comparer ?? EqualityComparer<TGroup>.Default);
            if (!hasStableKeys)
            {
                _itemToGroup = typeof(T).IsValueType
                    ? new Dictionary<T, ItemGroupInfo>()
                    : new Dictionary<T, ItemGroupInfo>((IEqualityComparer<T>) InternalEqualityComparer.Reference);
            }

            Priority = priority;
        }

        private bool HasUpdateHandler
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _itemToGroup != null || _updateGroup != null;
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
                if (HasUpdateHandler)
                {
                    var newGroup = _getGroup(t);
                    var updated = false;
                    if (_itemToGroup != null)
                    {
                        _itemToGroup.TryGetValue(t, out var info);
                        if (!_groups.Comparer.Equals(info.Group, newGroup!))
                        {
                            RemoveGroupIfNeed(decoratorManager, collection, info.Group, t);
                            AddGroupIfNeed(decoratorManager, collection, newGroup, t);
                            updated = true;
                        }
                    }

                    if (!updated)
                    {
                        if (newGroup != null)
                            UpdateGroup(newGroup, GroupHeaderChangedAction.ItemChanged, t, args);
                    }
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
            else if (oldItemGroup != null && HasUpdateHandler)
            {
                UpdateGroup(oldItemGroup, GroupHeaderChangedAction.ItemRemoved, (T) oldItem!, null);
                UpdateGroup(oldItemGroup, GroupHeaderChangedAction.ItemAdded, (T) newItem!, null);
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
            if (HasUpdateHandler)
            {
                foreach (var group in _groups)
                    UpdateGroup(group.Key, GroupHeaderChangedAction.Clear, default!, null);
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
            UpdateGroup(group, GroupHeaderChangedAction.ItemAdded, item, null);
        }

        private void RemoveGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TGroup? group, T item)
        {
            if (group == null)
                return;

            var groupInfo = _groups[group];
            if (--groupInfo.UsageCount != 0)
            {
                UpdateGroup(group, GroupHeaderChangedAction.ItemRemoved, item, null);
                return;
            }

            _groups.Remove(group);
            foreach (var info in _groups)
            {
                if (info.Value.Index > groupInfo.Index)
                    --info.Value.Index;
            }

            decoratorManager.OnRemoved(collection, this, group, groupInfo.Index);
            UpdateGroup(group, GroupHeaderChangedAction.Clear, item, null);
        }

        private void UpdateGroup(TGroup group, GroupHeaderChangedAction action, T item, object? args)
        {
            if (_itemToGroup != null)
            {
                if (action == GroupHeaderChangedAction.ItemAdded)
                {
                    _itemToGroup.TryGetValue(item, out var v);
                    _itemToGroup[item] = new ItemGroupInfo(group, v.UsageCount + 1);
                }
                else if (action == GroupHeaderChangedAction.ItemRemoved)
                {
                    if (_itemToGroup.TryGetValue(item, out var v))
                    {
                        if (v.UsageCount == 1)
                            _itemToGroup.Remove(item);
                        else
                            _itemToGroup[item] = new ItemGroupInfo(group, v.UsageCount - 1);
                    }
                }
                else if (action == GroupHeaderChangedAction.Clear)
                    _itemToGroup.Clear();
            }

            _updateGroup?.Invoke(group, action, item, args);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var group in _groups.OrderBy(pair => pair.Value.Index))
                yield return group.Key;
            foreach (var item in items)
                yield return item;
        }

        public delegate void UpdateGroupDelegate(TGroup group, GroupHeaderChangedAction action, T item, object? args);

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ItemGroupInfo
        {
            public readonly TGroup Group;
            public readonly int UsageCount;

            public ItemGroupInfo(TGroup group, int usageCount)
            {
                Group = group;
                UsageCount = usageCount;
            }
        }

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