using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

#pragma warning disable 8714

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class GroupCollectionDecorator<T, TKey, TGroup> : CollectionDecoratorBase
        where TGroup : class
    {
        private readonly Func<T, TKey?> _getKey;
        private readonly Func<TKey, TGroup> _getGroup;
        private readonly UpdateGroupDelegate<T, TGroup>? _updateGroup;
        private readonly Dictionary<TKey, (TGroup group, HashSetEx<T> items)> _groups;
        private IndexMapList<TKey> _keyMap;
        private ListInternal<(TKey key, TGroup group)> _groupList;
#if !NET5_0
        private List<TKey>? _oldKeys;
#endif

        public GroupCollectionDecorator(int priority, Func<T, TKey?> getKey, Func<TKey, TGroup> getGroup, UpdateGroupDelegate<T, TGroup>? updateGroup = null,
            IEqualityComparer<TKey>? comparer = null) : base(priority)
        {
            Should.NotBeNull(getKey, nameof(getKey));
            Should.NotBeNull(getGroup, nameof(getGroup));
            _getKey = getKey;
            _getGroup = getGroup;
            _updateGroup = updateGroup;
            _groups = new Dictionary<TKey, (TGroup, HashSetEx<T>)>(comparer ?? EqualityComparer<TKey>.Default);
            _groupList = new ListInternal<(TKey key, TGroup group)>(0);
            _keyMap = IndexMapList<TKey>.Get();
            Priority = priority;
        }

        public IReadOnlyCollection<TKey> Keys => _groups.Keys;

        protected override bool HasAdditionalItems => _groups.Count != 0;

        public bool TryGetGroup(TKey key, [NotNullWhen(true)] out TGroup? group, [NotNullWhen(true)] out IReadOnlyCollection<T>? items)
        {
            var result = _groups.TryGetValue(key, out var info);
            group = info.group;
            items = info.items;
            return result;
        }

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
            if (item is TGroup itemGroup)
            {
                var count = _groupList.Count;
                var groups = _groupList.Items;
                for (var i = 0; i < count; i++)
                {
                    if (EqualityComparer<TGroup>.Default.Equals(groups[i].group, itemGroup))
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
                var oldKey = binaryIndex < 0 ? default : _keyMap.Indexes[binaryIndex].Value;
                var newKey = _getKey(t);
                if (!_groups.Comparer.Equals(oldKey!, newKey!))
                    Replace(decoratorManager, collection, index, binaryIndex, newKey, item, t);
                else if (_updateGroup != null && oldKey != null && _groups.TryGetValue(oldKey, out var oldGroupInfo))
                    _updateGroup.Invoke(oldGroupInfo.group, oldGroupInfo.items, CollectionGroupChangedAction.ItemChanged, t, args);
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
                var key = _getKey(t);
                if (key != null)
                {
                    _keyMap.Add(index, key, binaryIndex);
                    decoratorManager.OnAdded(collection, this, item, index + _groups.Count);
                    AddGroupIfNeed(decoratorManager, collection, key, t);
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
                Replace(decoratorManager, collection, index, binaryIndex, _getKey(newItemT), oldItem, newItemT);

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
                    group.Value.items.Clear();

                var index = 0;
                foreach (var item in items)
                {
                    if (item is not T t)
                    {
                        ++index;
                        continue;
                    }

                    var key = _getKey(t);
                    if (key == null)
                    {
                        ++index;
                        continue;
                    }

                    if (_groups.TryGetValue(key, out var value))
                    {
                        if (value.items.Count == 0)
                            _groupList.Add((key, value.group));
                    }
                    else
                    {
                        var group = _getGroup(key);
                        value = (group, new HashSetEx<T>());
                        _groups[key] = value;
                        _groupList.Add((key, group));
                    }

                    _keyMap.AddRaw(index, key);
                    value.items.Add(t);
                    ++index;
                }


#if !NET5_0
                _oldKeys?.Clear();
#endif
                foreach (var group in _groups)
                {
                    if (group.Value.items.Count != 0)
                    {
                        _updateGroup?.Invoke(group.Value.group, group.Value.items, CollectionGroupChangedAction.Reset, default, null);
                        continue;
                    }

                    _updateGroup?.Invoke(group.Value.group, group.Value.items, CollectionGroupChangedAction.GroupRemoved, default, null);
#if NET5_0
                    _groups.Remove(group.Key);
#else
                    _oldKeys ??= new List<TKey>();
                    _oldKeys.Add(group.Key);
#endif
                }

#if !NET5_0
                if (_oldKeys != null)
                {
                    for (var i = 0; i < _oldKeys.Count; i++)
                        _groups.Remove(_oldKeys[i]);
                    _oldKeys.Clear();
                }
#endif
                items = Decorate(items);
            }

            return true;
        }

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, int index, int binaryIndex, TKey? newKey,
            object? oldItem, T newItemT)
        {
            if (newKey == null)
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
                    _keyMap.Add(index, newKey, binaryIndex);
                    AddGroupIfNeed(decoratorManager, collection, newKey, newItemT);
                }
                else
                {
                    var oldKey = _keyMap.Indexes[binaryIndex].Value;
                    if (_groups.Comparer.Equals(oldKey, newKey) && _groups.TryGetValue(oldKey, out var oldGroupInfo))
                    {
                        var oldItemT = (T) oldItem!;
                        oldGroupInfo.items.Remove(oldItemT);
                        _updateGroup?.Invoke(oldGroupInfo.group, oldGroupInfo.items, CollectionGroupChangedAction.ItemRemoved, oldItemT, null);

                        oldGroupInfo.items.Add(newItemT);
                        _updateGroup?.Invoke(oldGroupInfo.group, oldGroupInfo.items, CollectionGroupChangedAction.ItemAdded, newItemT, null);
                    }
                    else
                    {
                        _keyMap.Indexes[binaryIndex].Value = newKey;
                        RemoveGroupIfNeed(decoratorManager, collection, oldKey, (T) oldItem!);
                        AddGroupIfNeed(decoratorManager, collection, newKey, newItemT);
                    }
                }
            }
        }

        private void Clear()
        {
            if (_updateGroup != null)
            {
                foreach (var group in _groups)
                    _updateGroup(group.Value.group, group.Value.items, CollectionGroupChangedAction.GroupRemoved, default, null);
            }

            _groupList.Clear();
            _groups.Clear();
            _keyMap.Clear();
        }

        private void AddGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey key, T item)
        {
            if (!_groups.TryGetValue(key, out var groupInfo))
            {
                var group = _getGroup(key);
                _groupList.Add((key, group));
                groupInfo = (group, new HashSetEx<T>());
                _groups[key] = groupInfo;
                decoratorManager.OnAdded(collection, this, group, _groupList.Count - 1);
            }

            groupInfo.items.Add(item);
            _updateGroup?.Invoke(groupInfo.group, groupInfo.items, CollectionGroupChangedAction.ItemAdded, item, null);
        }

        private void RemoveGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey key, T item)
        {
            if (!_groups.TryGetValue(key, out var groupInfo))
                return;

            if (groupInfo.items.Count != 1)
            {
                if (groupInfo.items.Remove(item))
                    _updateGroup?.Invoke(groupInfo.group, groupInfo.items, CollectionGroupChangedAction.ItemRemoved, item, null);
                return;
            }

            _groups.Remove(key);
            var oldIndex = _groupList.IndexOf((key, groupInfo.group));
            _groupList.RemoveAt(oldIndex);

            decoratorManager.OnRemoved(collection, this, groupInfo.group, oldIndex);
            _updateGroup?.Invoke(groupInfo.group, groupInfo.items, CollectionGroupChangedAction.GroupRemoved, item, null);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            var count = _groupList.Count;
            var groups = _groupList.Items;
            for (var i = 0; i < count; i++)
                yield return groups[i].group;
            foreach (var item in items)
                yield return item;
        }
    }
}