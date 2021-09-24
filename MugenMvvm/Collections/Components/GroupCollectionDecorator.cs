using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

#pragma warning disable 8714

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class GroupCollectionDecorator<T, TKey> : CollectionDecoratorBase where TKey : class
    {
        private readonly Func<T, TKey?> _getGroup;
        private readonly UpdateGroupDelegate? _updateGroup;
        private readonly Dictionary<TKey, HashSetEx<T>> _groups;
        private IndexMapList<TKey> _keyMap;
        private ListInternal<TKey> _groupList;
#if !NET5_0
        private List<TKey>? _oldGroups;
#endif

        public GroupCollectionDecorator(int priority, Func<T, TKey?> getGroup, UpdateGroupDelegate? updateGroup = null,
            IEqualityComparer<TKey>? comparer = null) : base(priority)
        {
            Should.NotBeNull(getGroup, nameof(getGroup));
            _getGroup = getGroup;
            _updateGroup = updateGroup;
            _groups = new Dictionary<TKey, HashSetEx<T>>(comparer ?? EqualityComparer<TKey>.Default);
            _groupList = new ListInternal<TKey>(0);
            _keyMap = IndexMapList<TKey>.Get();
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
                for (var i = 0; i < count; i++)
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
                var binaryIndex = _keyMap.BinarySearch(index);
                var oldGroup = binaryIndex < 0 ? null : _keyMap.Indexes[binaryIndex].Value;
                var newGroup = _getGroup(t);
                if (!_groups.Comparer.Equals(oldGroup!, newGroup!))
                    Replace(decoratorManager, collection, index, binaryIndex, newGroup, item, t);
                else if (_updateGroup != null && oldGroup != null && _groups.TryGetValue(oldGroup, out var oldGroupInfo))
                    _updateGroup.Invoke(oldGroup, oldGroupInfo, CollectionGroupChangedAction.ItemChanged, t, args);
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var binaryIndex = _keyMap.BinarySearch(index);
            _keyMap.UpdateIndexesBinary(binaryIndex, 1);
            if (item is T t)
            {
                var group = _getGroup(t);
                if (group != null)
                {
                    _keyMap.Add(index, group, binaryIndex);
                    decoratorManager.OnAdded(collection, this, item, index + _groups.Count);
                    AddGroupIfNeed(decoratorManager, collection, group, t);
                    return false;
                }
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            var binaryIndex = _keyMap.BinarySearch(index);
            if (newItem is not T newItemT)
            {
                if (binaryIndex >= 0)
                {
                    _keyMap.RemoveAt(binaryIndex);
                    RemoveGroupIfNeed(decoratorManager, collection, _keyMap.Indexes[binaryIndex].Value, (T) oldItem!);
                }
            }
            else
                Replace(decoratorManager, collection, index, binaryIndex, _getGroup(newItemT), oldItem, newItemT);

            index += _groups.Count;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            _keyMap.Move(oldIndex, newIndex, out _);
            oldIndex += _groups.Count;
            newIndex += _groups.Count;
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var binaryIndex = _keyMap.BinarySearch(index);
            _keyMap.UpdateIndexesBinary(binaryIndex, -1);
            if (binaryIndex >= 0)
            {
                decoratorManager.OnRemoved(collection, this, item, index + _groups.Count);
                var oldGroup = _keyMap.Indexes[binaryIndex].Value;
                _keyMap.RemoveAt(binaryIndex);
                RemoveGroupIfNeed(decoratorManager, collection, oldGroup, (T) item!);
                return false;
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items.IsNullOrEmpty())
                Clear();
            else
            {
                _keyMap.Clear();
                _groupList.Clear();
                foreach (var group in _groups)
                    group.Value.Clear();

                var index = 0;
                foreach (var item in items)
                {
                    if (item is not T t)
                    {
                        ++index;
                        continue;
                    }

                    var group = _getGroup(t);
                    if (group == null)
                    {
                        ++index;
                        continue;
                    }

                    if (_groups.TryGetValue(group, out var value))
                    {
                        if (value.Count == 0)
                            _groupList.Add(group);
                    }
                    else
                    {
                        value = new HashSetEx<T>();
                        _groups[group] = value;
                        _groupList.Add(group);
                    }

                    _keyMap.AddRaw(index, group);
                    value.Add(t);
                    ++index;
                }

#if NET5_0
                foreach (var group in _groups)
                {
                    if (group.Value.Count != 0)
                    {
                        _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.Reset, default, null);
                        continue;
                    }

                    _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.Clear, default, null);
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

                    _updateGroup?.Invoke(group.Key, group.Value, CollectionGroupChangedAction.Clear, default, null);
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

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, int index, int binaryIndex, TKey? newGroup,
            object? oldItem, T newItemT)
        {
            if (newGroup == null)
            {
                if (binaryIndex >= 0)
                {
                    var oldGroup = _keyMap.Indexes[binaryIndex].Value;
                    _keyMap.RemoveAt(binaryIndex);
                    RemoveGroupIfNeed(decoratorManager, collection, oldGroup, (T) oldItem!);
                }
            }
            else
            {
                if (binaryIndex < 0)
                {
                    _keyMap.Add(index, newGroup, binaryIndex);
                    AddGroupIfNeed(decoratorManager, collection, newGroup, newItemT);
                }
                else
                {
                    var oldGroup = _keyMap.Indexes[binaryIndex].Value;
                    if (_groups.Comparer.Equals(oldGroup, newGroup) && _groups.TryGetValue(oldGroup, out var oldGroupInfo))
                    {
                        var oldItemT = (T) oldItem!;
                        oldGroupInfo.Remove(oldItemT);
                        _updateGroup?.Invoke(oldGroup, oldGroupInfo, CollectionGroupChangedAction.ItemRemoved, oldItemT, null);

                        oldGroupInfo.Add(newItemT);
                        _updateGroup?.Invoke(oldGroup, oldGroupInfo, CollectionGroupChangedAction.ItemAdded, newItemT, null);
                    }
                    else
                    {
                        _keyMap.Indexes[binaryIndex].Value = newGroup;
                        RemoveGroupIfNeed(decoratorManager, collection, oldGroup, (T) oldItem!);
                        AddGroupIfNeed(decoratorManager, collection, newGroup, newItemT);
                    }
                }
            }
        }

        private void Clear()
        {
            if (_updateGroup != null)
            {
                foreach (var group in _groups)
                    _updateGroup(group.Key, group.Value, CollectionGroupChangedAction.Clear, default, null);
            }

            _groupList.Clear();
            _groups.Clear();
            _keyMap.Clear();
        }

        private void AddGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey group, T item)
        {
            if (!_groups.TryGetValue(group, out var groupInfo))
            {
                _groupList.Add(group);
                groupInfo = new HashSetEx<T>();
                _groups[group] = groupInfo;
                decoratorManager.OnAdded(collection, this, group, _groupList.Count - 1);
            }

            groupInfo.Add(item);
            _updateGroup?.Invoke(group, groupInfo, CollectionGroupChangedAction.ItemAdded, item, null);
        }

        private void RemoveGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey group, T item)
        {
            if (!_groups.TryGetValue(group, out var groupInfo))
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
            _updateGroup?.Invoke(group, groupInfo, CollectionGroupChangedAction.Clear, item, null);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            var count = _groupList.Count;
            var groups = _groupList.Items;
            for (var i = 0; i < count; i++)
                yield return groups[i];
            foreach (var item in items)
                yield return item;
        }

        public delegate void UpdateGroupDelegate(TKey group, IReadOnlyCollection<T> groupItems, CollectionGroupChangedAction action, T? item, object? args);
    }
}