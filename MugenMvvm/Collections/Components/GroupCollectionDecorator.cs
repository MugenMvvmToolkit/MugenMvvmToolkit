using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

#pragma warning disable 8714

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class GroupCollectionDecorator<T, TKey> : CollectionDecoratorBase where TKey : class
    {
        private readonly Func<T, TKey?> _getGroup;
        private readonly UpdateGroupDelegate? _updateGroup;
        private readonly Dictionary<TKey, List<T>> _groups;
        private ListInternal<TKey> _groupList;
#if !NET5_0
        private List<TKey>? _oldGroups;
#endif

        public GroupCollectionDecorator(Func<T, TKey?> getGroup, UpdateGroupDelegate? updateGroup = null,
            IEqualityComparer<TKey>? comparer = null, int priority = CollectionComponentPriority.GroupHeaderDecorator) : base(priority)
        {
            Should.NotBeNull(getGroup, nameof(getGroup));
            _getGroup = getGroup;
            _updateGroup = updateGroup;
            _groups = new Dictionary<TKey, List<T>>(comparer ?? EqualityComparer<TKey>.Default);
            _groupList = new ListInternal<TKey>(0);
            Priority = priority;
        }

        protected override bool HasAdditionalItems => _groups.Count != 0;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            Clear();
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (item is TKey itemGroup)
            {
                var count = _groupList.Count;
                var groups = _groupList.Items;
                for (int i = 0; i < count; i++)
                {
                    if (_groups.Comparer.Equals(groups[i], itemGroup))
                    {
                        indexes.Add(i);
                        if (ignoreDuplicates)
                            return true;
                    }
                }
            }

            return true;
        }

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            if (item is T t)
            {
                var newGroup = _getGroup(t);
                if (newGroup == null || !_groups.TryGetValue(newGroup, out var oldGroupInfo) || !oldGroupInfo.Contains(t))
                {
                    TKey? oldGroup = null;
                    foreach (var group in _groups)
                    {
                        if (!_groups.Comparer.Equals(newGroup!, group.Key!) && group.Value.Contains(t))
                        {
                            oldGroup = group.Key;
                            break;
                        }
                    }

                    if (!_groups.Comparer.Equals(newGroup!, oldGroup!))
                    {
                        RemoveGroupIfNeed(decoratorManager, collection, oldGroup, t);
                        AddGroupIfNeed(decoratorManager, collection, newGroup, t);
                    }
                }
                else
                    _updateGroup?.Invoke(newGroup, oldGroupInfo, CollectionGroupChangedAction.ItemChanged, t, args);
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (item is T t)
                AddGroupIfNeed(decoratorManager, collection, _getGroup(t), t);

            index += _groups.Count;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var oldItemGroup = GetGroup(oldItem, out var oldGroupInfo);
            var newItemGroup = GetGroup(newItem, out _);
            if (!_groups.Comparer.Equals(oldItemGroup!, newItemGroup!))
            {
                RemoveGroupIfNeed(decoratorManager, collection, oldItemGroup, oldItemGroup == null ? default! : (T)oldItem!);
                AddGroupIfNeed(decoratorManager, collection, newItemGroup, newItemGroup == null ? default! : (T)newItem!);
            }
            else if (oldGroupInfo != null)
            {
                var oldItemT = (T)oldItem!;
                oldGroupInfo.Remove(oldItemT);
                _updateGroup?.Invoke(oldItemGroup!, oldGroupInfo, CollectionGroupChangedAction.ItemRemoved, oldItemT, null);

                var newItemT = (T)newItem!;
                oldGroupInfo.Add(newItemT);
                _updateGroup?.Invoke(oldItemGroup!, oldGroupInfo, CollectionGroupChangedAction.ItemAdded, newItemT, null);
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
                RemoveGroupIfNeed(decoratorManager, collection, _getGroup(t), t);

            index += _groups.Count;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items == null)
                Clear();
            else
            {
                _groupList.Clear();
                foreach (var group in _groups)
                    group.Value.Clear();

                foreach (var item in items)
                {
                    if (item is not T t)
                        continue;

                    var group = _getGroup(t);
                    if (group == null)
                        continue;

                    if (_groups.TryGetValue(group, out var value))
                    {
                        if (value.Count == 0)
                            _groupList.Add(group);
                    }
                    else
                    {
                        value = new List<T>();
                        _groups[group] = value;
                        _groupList.Add(group);
                    }

                    value.Add(t);
                }

#if NET5_0
                foreach (var group in _groups)
                {
                    if (group.Value.Count != 0)
                    {
                        _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.Reset, default, null);
                        continue;
                    }

                    _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.GroupRemoved, default, null);
                    _groups.Remove(group.Key);
                }
#else
                _oldGroups?.Clear();
                foreach (var group in _groups)
                {
                    if (group.Value.Count != 0)
                    {
                        _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.Reset, default, null);
                        continue;
                    }

                    _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.GroupRemoved, default, null);
                    _oldGroups ??= new List<TKey>();
                    _oldGroups.Add(group.Key);
                }

                if (_oldGroups != null)
                {
                    for (var i = 0; i < _oldGroups.Count; i++)
                        _groups.Remove(_oldGroups[i]);
                    _oldGroups.Clear();
                }
#endif

                items = Decorate(items);
            }

            return true;
        }

        private TKey? GetGroup(object? item, out List<T>? groupInfo)
        {
            if (item is T itemT)
            {
                var group = _getGroup(itemT);
                if (group != null && _groups.TryGetValue(group, out groupInfo))
                    return group;
            }

            groupInfo = null;
            return null;
        }

        private void Clear()
        {
            if (_updateGroup != null)
            {
                foreach (var group in _groups)
                    _updateGroup(group.Key, group.Value, CollectionGroupChangedAction.GroupRemoved, default, null);
            }

            _groupList.Clear();
            _groups.Clear();
        }

        private void AddGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey? group, T item)
        {
            if (group == null)
                return;

            if (!_groups.TryGetValue(group, out var groupInfo))
            {
                _groupList.Add(group);
                groupInfo = new List<T>();
                _groups[group] = groupInfo;
                decoratorManager.OnAdded(collection, this, group, _groupList.Count - 1);
            }

            groupInfo.Add(item);
            _updateGroup?.Invoke(group, groupInfo, CollectionGroupChangedAction.ItemAdded, item, null);
        }

        private void RemoveGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey? group, T item)
        {
            if (group == null || !_groups.TryGetValue(group, out var groupInfo))
                return;

            if (groupInfo.Count != 1)
            {
                if (groupInfo.Remove(item))
                    _updateGroup?.Invoke(group, groupInfo, CollectionGroupChangedAction.ItemRemoved, item, null);
                return;
            }

            _groups.Remove(group);
            var oldIndex = _groupList.IndexOf(group);
            _groupList.RemoveAt(oldIndex);

            decoratorManager.OnRemoved(collection, this, group, oldIndex);
            _updateGroup?.Invoke(group, groupInfo, CollectionGroupChangedAction.GroupRemoved, item, null);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            var count = _groupList.Count;
            var groups = _groupList.Items;
            for (int i = 0; i < count; i++)
                yield return groups[i];
            foreach (var item in items)
                yield return item;
        }

        public delegate void UpdateGroupDelegate(TKey group, IReadOnlyCollection<T> groupItems, CollectionGroupChangedAction action, T? item, object? args);
    }
}