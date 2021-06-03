using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class FlattenCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private readonly Func<object, FlattenItemInfo> _getNestedCollection;
        private readonly Dictionary<object, FlattenCollectionItemBase> _collectionItems;

        public FlattenCollectionDecorator(Func<object, FlattenItemInfo> getNestedCollection, int priority = CollectionComponentPriority.FlattenCollectionDecorator)
        {
            Should.NotBeNull(getNestedCollection, nameof(getNestedCollection));
            _getNestedCollection = getNestedCollection;
            Priority = priority;
            _collectionItems = new Dictionary<object, FlattenCollectionItemBase>(InternalEqualityComparer.Reference);
        }

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            Clear();
            DecoratorManager = null;
        }

        private int UpdateIndexes(int index, int value)
        {
            var result = index;
            foreach (var collectionItem in _collectionItems)
            {
                var item = collectionItem.Value;
                var items = item.Items;
                for (var i = 0; i < item.Count; i++)
                {
                    if (items[i] >= index)
                        items[i] += value;
                    else
                        result += item.Size - 1;
                }
            }

            return result;
        }

        private void Clear()
        {
            foreach (var collectionItem in _collectionItems)
                collectionItem.Value.Detach();
            _collectionItems.Clear();
        }

        private bool OnAdded(object? item, ref int index, bool notify = true)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, 1);
            if (item == null)
                return true;

            if (!_collectionItems.TryGetValue(item, out var flattenCollectionItem))
            {
                var flattenItemInfo = _getNestedCollection(item);
                if (flattenItemInfo.IsEmpty)
                    return true;

                flattenCollectionItem = flattenItemInfo.GetCollectionItem(this);
                _collectionItems[item] = flattenCollectionItem;
            }

            flattenCollectionItem.OnAdded(originalIndex, index, notify);
            return false;
        }

        private bool OnRemoved(object? item, ref int index)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, -1);
            if (!TryGetCollectionItem(item, out var flattenCollectionItem))
                return true;

            if (flattenCollectionItem.OnRemoved(originalIndex - 1, index))
                _collectionItems.Remove(item);
            return false;
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?> items)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    var flattenItemInfo = _getNestedCollection(item);
                    if (!flattenItemInfo.IsEmpty)
                    {
                        foreach (var nestedItem in flattenItemInfo.GetItems())
                            yield return nestedItem;
                        continue;
                    }
                }

                yield return item;
            }
        }

        private bool TryGetCollectionItem([NotNullWhen(true)] object? item, [NotNullWhen(true)] out FlattenCollectionItemBase? value)
        {
            if (item != null && _collectionItems.TryGetValue(item, out value))
                return true;
            value = null;
            return false;
        }

        private int GetIndex(int originalIndex)
        {
            var result = originalIndex;
            foreach (var collectionItem in _collectionItems)
            {
                var item = collectionItem.Value;
                var items = item.Items;
                for (var i = 0; i < item.Count; i++)
                {
                    if (items[i] < originalIndex)
                        result += item.Size - 1;
                }
            }

            return result;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => DecoratorManager == null ? items : Decorate(items);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (DecoratorManager == null || item != null && _collectionItems.ContainsKey(item))
                return false;

            index = GetIndex(index);
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            using var _ = DecoratorManager.BatchUpdate(collection, this);
            return OnAdded(item, ref index);
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            using var _ = DecoratorManager.BatchUpdate(collection, this);
            var addIndex = index;
            var removed = OnRemoved(oldItem, ref index);
            if (removed)
            {
                if (newItem != null && !_getNestedCollection(newItem).IsEmpty)
                {
                    DecoratorManager.OnRemoved(collection, this, oldItem, index);
                    removed = false;
                }
            }

            var added = OnAdded(newItem, ref addIndex);
            if (added && removed)
                return true;

            if (removed)
                DecoratorManager.OnRemoved(collection, this, oldItem, index);
            if (added)
                DecoratorManager.OnAdded(collection, this, newItem, index);
            return false;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (DecoratorManager == null)
                return false;

            using var _ = DecoratorManager.BatchUpdate(collection, this);
            if (TryGetCollectionItem(item, out var flattenCollectionItem))
                flattenCollectionItem.Remove(oldIndex);

            var originalNewIndex = newIndex;
            oldIndex = UpdateIndexes(oldIndex, -1);
            newIndex = UpdateIndexes(newIndex, 1);
            if (flattenCollectionItem == null)
                return true;

            flattenCollectionItem.Add(originalNewIndex);
            flattenCollectionItem.OnMoved(oldIndex, newIndex);
            return false;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            using var _ = DecoratorManager.BatchUpdate(collection, this);
            return OnRemoved(item, ref index);
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return false;

            Clear();
            if (items != null)
            {
                var index = 0;
                var i = 0;
                foreach (var item in items)
                {
                    OnAdded(item, ref i, false);
                    i = ++index;
                }

                items = Decorate(items);
            }

            return true;
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct FlattenItemInfo
        {
            internal readonly IEnumerable? Items;
            internal readonly bool DecoratorListener;

            public FlattenItemInfo(IEnumerable? items, bool decoratorListener = true)
            {
                Items = items;
                DecoratorListener = decoratorListener;
            }

            internal bool IsEmpty => Items == null;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal IEnumerable<object?> GetItems() => DecoratorListener ? Items!.Decorate() : Items!.AsEnumerable();

            internal FlattenCollectionItemBase GetCollectionItem(FlattenCollectionDecorator decorator)
            {
                if (DecoratorListener)
                    return new DecoratorFlattenCollectionItem(Items!, decorator);

                var itemType = MugenExtensions.GetCollectionItemType(Items!);
                if (!itemType.IsValueType)
                    return new SourceFlattenCollectionItem<object?>().Initialize(Items!, decorator);

                return ((FlattenCollectionItemBase)Activator.CreateInstance(typeof(SourceFlattenCollectionItem<>).MakeGenericType(itemType))!).Initialize(Items!, decorator);
            }
        }

        internal abstract class FlattenCollectionItemBase : ListInternal<int>, IAttachableComponent, ISynchronizationListener, IHasPriority
        {
            protected IEnumerable Collection = null!;
            private FlattenCollectionDecorator _decorator = null!;
            private ListInternal<(ActionToken token, bool batch)>? _tokens;

            protected FlattenCollectionItemBase(IEnumerable collection, FlattenCollectionDecorator decorator) : base(1)
            {
                Initialize(collection, decorator);
            }

            protected FlattenCollectionItemBase() : base(1)
            {
            }

            public int Size { get; private set; }

            public int Priority => CollectionComponentPriority.BindableAdapter;

            private ICollectionDecoratorManagerComponent? DecoratorManager => _decorator.DecoratorManager;

            public FlattenCollectionItemBase Initialize(IEnumerable collection, FlattenCollectionDecorator decorator)
            {
                _decorator = decorator;
                Collection = collection;
                return this;
            }

            public void OnAdded(int originalIndex, int index, bool notify)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                if (notify)
                {
                    Size = 0;
                    foreach (var item in GetItems())
                    {
                        DecoratorManager!.OnAdded(_decorator.Owner, _decorator, item, index + Size);
                        ++Size;
                    }
                }
                else if (Size == 0)
                    Size = GetItems().CountEx();

                AddOrdered(originalIndex, Comparer<int>.Default);
                if (Count == 1 && Collection is IComponentOwner<ICollection> owner)
                    owner.Components.Add(this);
            }

            public bool OnRemoved(int originalIndex, int index)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                foreach (var item in GetItems())
                    DecoratorManager!.OnRemoved(_decorator.Owner, _decorator, item, index);

                Remove(originalIndex);
                if (Count == 0)
                {
                    Detach();
                    return true;
                }

                return false;
            }

            public void OnMoved(int oldIndex, int newIndex)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                var removeIndex = 0;
                var addIndex = 0;
                if (oldIndex < newIndex)
                    addIndex += Size - 1;
                foreach (var item in GetItems())
                {
                    DecoratorManager!.OnMoved(_decorator.Owner, _decorator, item, oldIndex + removeIndex, newIndex + addIndex);
                    if (oldIndex > newIndex)
                    {
                        ++removeIndex;
                        ++addIndex;
                    }
                }
            }

            public void Detach()
            {
                Clear();
                if (Collection is IComponentOwner<ICollection> owner)
                    owner.Components.Remove(this);
                var tokens = _tokens;
                if (tokens != null)
                {
                    _tokens = null;
                    for (int i = 0; i < tokens.Count; i++)
                        tokens.Items[i].token.Dispose();
                }
            }

            public void OnChanged(ICollection collection, object? item, int index, object? args)
            {
                if (DecoratorManager == null)
                    return;

                using var _ = BatchIfNeed();
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnChanged(_decorator.Owner, _decorator, item, _decorator.GetIndex(Items[i]) + index, args);
            }

            public void OnAdded(ICollection collection, object? item, int index)
            {
                if (DecoratorManager == null)
                    return;

                using var _ = BatchIfNeed();
                ++Size;
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnAdded(_decorator.Owner, _decorator, item, _decorator.GetIndex(Items[i]) + index);
            }

            public void OnReplaced(ICollection collection, object? oldItem, object? newItem, int index)
            {
                if (DecoratorManager == null)
                    return;

                using var _ = BatchIfNeed();
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnReplaced(_decorator.Owner, _decorator, oldItem, newItem, _decorator.GetIndex(Items[i]) + index);
            }

            public void OnMoved(ICollection collection, object? item, int oldIndex, int newIndex)
            {
                if (DecoratorManager == null)
                    return;

                using var _ = BatchIfNeed();
                for (var i = 0; i < Count; i++)
                {
                    var originalIndex = _decorator.GetIndex(Items[i]);
                    DecoratorManager.OnMoved(_decorator.Owner, _decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
                }
            }

            public void OnRemoved(ICollection collection, object? item, int index)
            {
                if (DecoratorManager == null)
                    return;

                using var _ = BatchIfNeed();
                --Size;
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnRemoved(_decorator.Owner, _decorator, item, _decorator.GetIndex(Items[i]) + index);
            }

            public void OnReset(ICollection collection, IEnumerable<object?>? items)
            {
                if (DecoratorManager == null)
                    return;

                Size = items.CountEx();
                DecoratorManager.OnReset(_decorator.Owner, _decorator, _decorator.Decorate(DecoratorManager.Decorate(_decorator.Owner, _decorator)));
            }

            public void OnBeginBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                if (DecoratorManager != null)
                    AddToken(DecoratorManager.BatchUpdate(_decorator.Owner, _decorator), true);
            }

            public void OnEndBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                RemoveToken(true);
            }

            protected abstract IEnumerable<object?> GetItems();

            private void AddToken(ActionToken actionToken, bool batch)
            {
                _tokens ??= new ListInternal<(ActionToken token, bool batch)>(2);
                _tokens.Add((actionToken, batch));
            }

            private void RemoveToken(bool batch)
            {
                if (_tokens == null)
                    return;

                var items = _tokens.Items;
                for (var i = _tokens.Count - 1; i >= 0; i--)
                {
                    if (items[i].batch == batch)
                    {
                        var actionToken = items[i].token;
                        _tokens.RemoveAt(i);
                        actionToken.Dispose();
                        return;
                    }
                }
            }

            private ActionToken BatchIfNeed() => Count > 1 ? DecoratorManager!.BatchUpdate(_decorator.Owner, _decorator) : default;

            bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

            void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata) => CollectionDecoratorManager.GetOrAdd((IEnumerable)owner);

            void ISynchronizationListener.OnLocking(object target, IReadOnlyMetadataContext? metadata) => AddToken(MugenExtensions.TryLock(_decorator.OwnerOptional), false);

            void ISynchronizationListener.OnLocked(object target, IReadOnlyMetadataContext? metadata)
            {
            }

            void ISynchronizationListener.OnUnlocking(object target, IReadOnlyMetadataContext? metadata)
            {
            }

            void ISynchronizationListener.OnUnlocked(object target, IReadOnlyMetadataContext? metadata) => RemoveToken(false);
        }

        private sealed class SourceFlattenCollectionItem<T> : FlattenCollectionItemBase, ICollectionChangedListener<T>
        {
            private static IEnumerable<object?>? AsObjectEnumerable(IEnumerable<T>? items) => items == null ? null : items as IEnumerable<object?> ?? items.Cast<object>();

            public void OnChanged(IReadOnlyCollection<T> collection, T item, int index, object? args) => OnChanged((ICollection)collection, item, index, args);

            public void OnAdded(IReadOnlyCollection<T> collection, T item, int index) => OnAdded((ICollection)collection, item, index);

            public void OnReplaced(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index) =>
                OnReplaced((ICollection)collection, oldItem, newItem, index);

            public void OnMoved(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex) => OnMoved((ICollection)collection, item, oldIndex, newIndex);

            public void OnRemoved(IReadOnlyCollection<T> collection, T item, int index) => OnRemoved((ICollection)collection, item, index);

            public void OnReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items) => OnReset((ICollection)collection, AsObjectEnumerable(items));

            protected override IEnumerable<object?> GetItems() => Collection.AsEnumerable();
        }

        private sealed class DecoratorFlattenCollectionItem : FlattenCollectionItemBase, ICollectionDecoratorListener, ICollectionBatchUpdateListener
        {
            public DecoratorFlattenCollectionItem(IEnumerable collection, FlattenCollectionDecorator decorator) : base(collection, decorator)
            {
            }

            protected override IEnumerable<object?> GetItems() => Collection.Decorate();
        }
    }
}