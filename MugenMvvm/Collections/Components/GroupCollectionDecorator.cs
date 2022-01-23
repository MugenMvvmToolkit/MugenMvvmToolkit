using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

#pragma warning disable 8714

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class GroupCollectionDecorator<T, TKey, TGroup> : CollectionDecoratorBase
        where TKey : notnull
        where TGroup : class
    {
        private readonly bool _allowNull;
        private readonly Func<T, Optional<TKey>> _getKey;
        private readonly Func<TKey, TGroup> _getGroup;
        private readonly UpdateGroupDelegate<T, TKey, TGroup>? _updateGroup;
        private readonly IEqualityComparer<T>? _comparerValue;
        private readonly Dictionary<TKey, (TGroup group, HashSetEx<T> items)> _groups;
        private IndexMapList<TKey> _keyMap;
        private ListInternal<(TKey key, TGroup group)> _groupList;
#if !NET5_0
        private List<TKey>? _oldKeys;
#endif

        public GroupCollectionDecorator(int priority, bool allowNull, Func<T, Optional<TKey>> getKey, Func<TKey, TGroup> getGroup,
            UpdateGroupDelegate<T, TKey, TGroup>? updateGroup, IEqualityComparer<TKey>? comparer, IEqualityComparer<T>? comparerValue) : base(priority)
        {
            Should.NotBeNull(getKey, nameof(getKey));
            Should.NotBeNull(getGroup, nameof(getGroup));
            _allowNull = allowNull && TypeChecker.IsNullable<T>();
            _getKey = getKey;
            _getGroup = getGroup;
            _updateGroup = updateGroup;
            _comparerValue = comparerValue;
            _groups = new Dictionary<TKey, (TGroup group, HashSetEx<T> items)>(comparer);
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
            if (item.TryCast<T>(_allowNull, out var t))
            {
                var binaryIndex = _keyMap.BinarySearch(index);
                var oldKey = binaryIndex < 0 ? default : _keyMap.Indexes[binaryIndex].Value;
                var newKey = _getKey(t!);
                if (binaryIndex >= 0 != newKey.HasNonNullValue || !_groups.Comparer.Equals(oldKey!, newKey.GetValueOrDefault()!))
                    Replace(decoratorManager, collection, index, binaryIndex, newKey, item, t!);
                else if (_updateGroup != null && binaryIndex >= 0 && _groups.TryGetValue(oldKey!, out var oldGroupInfo))
                    _updateGroup.Invoke(oldKey!, oldGroupInfo.group, oldGroupInfo.items, CollectionGroupChangedAction.ItemChanged, t, args);
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var binaryIndex = _keyMap.BinarySearch(index);
            _keyMap.UpdateIndexesBinary(binaryIndex, 1);
            if (item.TryCast<T>(_allowNull, out var t))
            {
                var key = _getKey(t!);
                if (key.HasNonNullValue)
                {
                    _keyMap.Add(index, key.Value, binaryIndex);
                    decoratorManager.OnAdded(collection, this, item, index + _groups.Count);
                    AddGroupIfNeed(decoratorManager, collection, key.Value, t!);
                    return false;
                }
            }

            index += _groups.Count;
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (oldItem.TryCheckCast<T>(_allowNull) || newItem.TryCheckCast<T>(_allowNull))
            {
                var binaryIndex = _keyMap.BinarySearch(index);
                if (newItem.TryCast<T>(_allowNull, out var newItemT))
                {
                    decoratorManager.OnReplaced(collection, this, oldItem, newItem, index + _groups.Count);
                    Replace(decoratorManager, collection, index, binaryIndex, _getKey(newItemT!), oldItem, newItemT!);
                    return false;
                }

                if (binaryIndex >= 0)
                {
                    decoratorManager.OnReplaced(collection, this, oldItem, newItem, index + _groups.Count);
                    _keyMap.RemoveAt(binaryIndex);
                    RemoveGroupIfNeed(decoratorManager, collection, _keyMap.Indexes[binaryIndex].Value, (T) oldItem!);
                    return false;
                }
            }

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
                    if (!item.TryCast<T>(_allowNull, out var t))
                    {
                        ++index;
                        continue;
                    }

                    var key = _getKey(t!);
                    if (!key.HasNonNullValue)
                    {
                        ++index;
                        continue;
                    }

                    if (_groups.TryGetValue(key.Value, out var value))
                    {
                        if (value.items.Count == 0)
                            _groupList.Add((key.Value, value.group));
                    }
                    else
                    {
                        var group = _getGroup(key.Value);
                        value = (group, new HashSetEx<T>(_comparerValue));
                        _groups[key.Value] = value;
                        _groupList.Add((key.Value, group));
                    }

                    _keyMap.AddRaw(index, key.Value);
                    value.items.Add(t!);
                    ++index;
                }


#if !NET5_0
                _oldKeys?.Clear();
#endif
                foreach (var group in _groups)
                {
                    if (group.Value.items.Count != 0)
                    {
                        _updateGroup?.Invoke(group.Key, group.Value.group, group.Value.items, CollectionGroupChangedAction.Reset, default, null);
                        continue;
                    }

                    _updateGroup?.Invoke(group.Key, group.Value.group, group.Value.items, CollectionGroupChangedAction.GroupRemoved, default, null);
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

        private void Replace(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, int index, int binaryIndex, Optional<TKey> newKey,
            object? oldItem, T newItemT)
        {
            if (newKey.HasNonNullValue)
            {
                if (binaryIndex < 0)
                {
                    _keyMap.Add(index, newKey.Value, binaryIndex);
                    AddGroupIfNeed(decoratorManager, collection, newKey.Value, newItemT);
                }
                else
                {
                    var oldKey = _keyMap.Indexes[binaryIndex].Value;
                    if (_groups.Comparer.Equals(oldKey, newKey.Value) && _groups.TryGetValue(oldKey, out var oldGroupInfo))
                    {
                        var oldItemT = (T) oldItem!;
                        oldGroupInfo.items.Remove(oldItemT);
                        _updateGroup?.Invoke(oldKey, oldGroupInfo.group, oldGroupInfo.items, CollectionGroupChangedAction.ItemRemoved, oldItemT, null);

                        oldGroupInfo.items.Add(newItemT);
                        _updateGroup?.Invoke(oldKey, oldGroupInfo.group, oldGroupInfo.items, CollectionGroupChangedAction.ItemAdded, newItemT, null);
                    }
                    else
                    {
                        _keyMap.Indexes[binaryIndex].Value = newKey.Value;
                        RemoveGroupIfNeed(decoratorManager, collection, oldKey, (T) oldItem!);
                        AddGroupIfNeed(decoratorManager, collection, newKey.Value, newItemT);
                    }
                }

                return;
            }

            if (binaryIndex >= 0)
            {
                var oldGroup = _keyMap.Indexes[binaryIndex].Value;
                _keyMap.RemoveAt(binaryIndex);
                RemoveGroupIfNeed(decoratorManager, collection, oldGroup, (T) oldItem!);
            }
        }

        private void Clear()
        {
            if (_updateGroup != null)
            {
                foreach (var group in _groups)
                    _updateGroup(group.Key, group.Value.group, group.Value.items, CollectionGroupChangedAction.GroupRemoved, default, null);
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
                groupInfo = (group, new HashSetEx<T>(_comparerValue));
                _groups[key] = groupInfo;
                decoratorManager.OnAdded(collection, this, group, _groupList.Count - 1);
            }

            groupInfo.items.Add(item);
            _updateGroup?.Invoke(key, groupInfo.group, groupInfo.items, CollectionGroupChangedAction.ItemAdded, item, null);
        }

        private void RemoveGroupIfNeed(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, TKey key, T item)
        {
            if (!_groups.TryGetValue(key, out var groupInfo))
                return;

            if (groupInfo.items.Count != 1)
            {
                if (groupInfo.items.Remove(item))
                    _updateGroup?.Invoke(key, groupInfo.group, groupInfo.items, CollectionGroupChangedAction.ItemRemoved, item, null);
                return;
            }

            _groups.Remove(key);
            var oldIndex = _groupList.IndexOf((key, groupInfo.group));
            _groupList.RemoveAt(oldIndex);

            decoratorManager.OnRemoved(collection, this, groupInfo.group, oldIndex);
            _updateGroup?.Invoke(key, groupInfo.group, groupInfo.items, CollectionGroupChangedAction.GroupRemoved, item, null);
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