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
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class FlattenCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority, ILockerChangedListener
    {
        private readonly Func<object, FlattenItemInfo> _getNestedCollection;
        private readonly Dictionary<object, FlattenCollectionItemBase> _collectionItems;
        private int? _batchLimit;

        public FlattenCollectionDecorator(Func<object, FlattenItemInfo> getNestedCollection, int priority = CollectionComponentPriority.FlattenCollectionDecorator)
        {
            Should.NotBeNull(getNestedCollection, nameof(getNestedCollection));
            _getNestedCollection = getNestedCollection;
            Priority = priority;
            _collectionItems = new Dictionary<object, FlattenCollectionItemBase>(InternalEqualityComparer.Reference);
        }

        public int BatchLimit
        {
            get => _batchLimit.GetValueOrDefault(CollectionMetadata.FlattenCollectionDecoratorBatchLimit);
            set => _batchLimit = value;
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

        private bool OnAdded(object source, object? item, ref int index, bool notify, out bool isReset)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, 1);
            if (item == null)
            {
                isReset = false;
                return true;
            }

            var isRecycled = true;
            if (!_collectionItems.TryGetValue(item, out var flattenCollectionItem))
            {
                var flattenItemInfo = _getNestedCollection(item);
                if (flattenItemInfo.IsEmpty)
                {
                    isReset = false;
                    return true;
                }

                flattenCollectionItem = flattenItemInfo.GetCollectionItem(this);
                _collectionItems[item] = flattenCollectionItem;
                isRecycled = false;
            }

            flattenCollectionItem.OnAdded(source, originalIndex, index, notify, isRecycled, out isReset);
            return false;
        }

        private bool OnRemoved(object? item, ref int index, out bool isReset)
        {
            var originalIndex = index;
            index = UpdateIndexes(index, -1);
            if (!TryGetCollectionItem(item, out var flattenCollectionItem))
            {
                isReset = false;
                return true;
            }

            if (flattenCollectionItem.OnRemoved(originalIndex - 1, index, out isReset))
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

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index) => DecoratorManager != null && OnAdded(collection, item, ref index, true, out _);

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            using var t = DecoratorManager.BatchUpdate(collection, this);
            var addIndex = index;
            var removed = OnRemoved(oldItem, ref index, out var isRemoveReset);
            if (removed)
            {
                if (newItem != null && !_getNestedCollection(newItem).IsEmpty)
                {
                    DecoratorManager.OnRemoved(collection, this, oldItem, index);
                    removed = false;
                }
            }

            var added = OnAdded(collection, newItem, ref addIndex, !isRemoveReset, out var isAddReset);
            if (added && removed)
                return true;
            if (isAddReset || isRemoveReset)
                return false;

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

            if (TryGetCollectionItem(item, out var flattenCollectionItem))
                flattenCollectionItem.Remove(oldIndex);

            var originalNewIndex = newIndex;
            oldIndex = UpdateIndexes(oldIndex, -1);
            newIndex = UpdateIndexes(newIndex, 1);
            if (flattenCollectionItem == null)
                return true;

            flattenCollectionItem.Add(originalNewIndex);
            if (oldIndex != newIndex)
                flattenCollectionItem.OnMoved(oldIndex, newIndex);
            return false;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index) => DecoratorManager != null && OnRemoved(item, ref index, out _);

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return false;

            Clear();
            if (items == null)
                Clear();
            else
            {
                foreach (var collectionItem in _collectionItems)
                    collectionItem.Value.Clear();

                var index = 0;
                var i = 0;
                foreach (var item in items)
                {
                    OnAdded(collection, item, ref i, false, out _);
                    i = ++index;
                }

#if !NET5_0
                var toRemove = new ItemOrListEditor<object>();
#endif
                foreach (var item in _collectionItems)
                {
                    if (item.Value.Count == 0)
                    {
#if NET5_0
                        _collectionItems.Remove(item.Key);
#else
                        toRemove.Add(item.Key);
#endif
                        item.Value.Detach();
                    }
                }
#if !NET5_0
                foreach (object item in toRemove.ToItemOrList())
                    _collectionItems.Remove(item);
#endif

                items = Decorate(items);
            }

            return true;
        }

        void ILockerChangedListener.OnChanged(object owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            using var _ = MugenExtensions.TryLock(owner);
            foreach (var item in _collectionItems)
                item.Value.UpdateLocker(locker);
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

        internal abstract class FlattenCollectionItemBase : ListInternal<int>, IAttachableComponent, ILockerChangedListener, IHasPriority
        {
            protected IEnumerable Collection = null!;
            protected FlattenCollectionDecorator Decorator = null!;
            private ListInternal<ActionToken>? _tokens;
            private bool _detached;

            protected FlattenCollectionItemBase(IEnumerable collection, FlattenCollectionDecorator decorator) : base(1)
            {
                Initialize(collection, decorator);
            }

            protected FlattenCollectionItemBase() : base(1)
            {
            }

            public int Size { get; private set; }

            public int Priority => CollectionComponentPriority.BindableAdapter;

            protected ICollectionDecoratorManagerComponent? DecoratorManager => Decorator.DecoratorManager;

            public FlattenCollectionItemBase Initialize(IEnumerable collection, FlattenCollectionDecorator decorator)
            {
                Decorator = decorator;
                Collection = collection;
                return this;
            }

            public void UpdateLocker(ILocker locker)
            {
                if (Collection is ISynchronizable synchronizable)
                    synchronizable.UpdateLocker(locker);
            }

            public void OnAdded(object source, int originalIndex, int index, bool notify, bool isRecycled, out bool isReset)
            {
                isReset = false;
                using var _ = MugenExtensions.TryLock(Collection);
                if (notify)
                {
                    Size = GetItems().Count();
                    if (Size > Decorator.BatchLimit)
                    {
                        isReset = true;
                        Reset();
                    }
                    else
                    {
                        foreach (var item in GetItems())
                            DecoratorManager!.OnAdded(Decorator.Owner, Decorator, item, index++);
                    }
                }
                else if (Size == 0)
                    Size = GetItems().CountEx();

                AddOrdered(originalIndex, Comparer<int>.Default);
                if (!isRecycled)
                {
                    if (Collection is IComponentOwner<ICollection> owner)
                        owner.Components.Add(this);

                    if (Collection is ISynchronizable collectionSynchronizable && source is ISynchronizable sourceSynchronizable)
                    {
                        collectionSynchronizable.UpdateLocker(sourceSynchronizable.Locker);
                        sourceSynchronizable.UpdateLocker(collectionSynchronizable.Locker);
                    }
                }
            }

            public bool OnRemoved(int originalIndex, int index, out bool isReset)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                if (Size > Decorator.BatchLimit)
                {
                    isReset = true;
                    Reset();
                }
                else
                {
                    isReset = false;
                    foreach (var item in GetItems())
                        DecoratorManager!.OnRemoved(Decorator.Owner, Decorator, item, index);
                }

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
                if (Size > Decorator.BatchLimit)
                {
                    Reset();
                    return;
                }

                var removeIndex = 0;
                var addIndex = 0;
                if (oldIndex < newIndex)
                    addIndex += Size - 1;
                foreach (var item in GetItems())
                {
                    DecoratorManager!.OnMoved(Decorator.Owner, Decorator, item, oldIndex + removeIndex, newIndex + addIndex);
                    if (oldIndex > newIndex)
                    {
                        ++removeIndex;
                        ++addIndex;
                    }
                }
            }

            public void Detach()
            {
                _detached = true;
                Clear();
                if (Collection is IComponentOwner<ICollection> owner)
                    owner.Components.Remove(this);

                var tokens = _tokens;
                if (tokens != null)
                {
                    _tokens = null;
                    for (var i = 0; i < tokens.Count; i++)
                        tokens.Items[i].Dispose();
                }
            }

            public void OnChanged(ICollection collection, object? item, int index, object? args)
            {
                if (DecoratorManager == null)
                    return;

                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnChanged(Decorator.Owner, Decorator, item, Decorator.GetIndex(Items[i]) + index, args);
            }

            public void OnAdded(ICollection collection, object? item, int index)
            {
                if (DecoratorManager == null)
                    return;

                ++Size;
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnAdded(Decorator.Owner, Decorator, item, Decorator.GetIndex(Items[i]) + index);
            }

            public void OnReplaced(ICollection collection, object? oldItem, object? newItem, int index)
            {
                if (DecoratorManager == null)
                    return;

                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnReplaced(Decorator.Owner, Decorator, oldItem, newItem, Decorator.GetIndex(Items[i]) + index);
            }

            public void OnMoved(ICollection collection, object? item, int oldIndex, int newIndex)
            {
                if (DecoratorManager == null)
                    return;

                for (var i = 0; i < Count; i++)
                {
                    var originalIndex = Decorator.GetIndex(Items[i]);
                    DecoratorManager.OnMoved(Decorator.Owner, Decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
                }
            }

            public void OnRemoved(ICollection collection, object? item, int index)
            {
                if (DecoratorManager == null)
                    return;

                --Size;
                for (var i = 0; i < Count; i++)
                    DecoratorManager.OnRemoved(Decorator.Owner, Decorator, item, Decorator.GetIndex(Items[i]) + index);
            }

            public void OnReset(ICollection collection, IEnumerable<object?>? items)
            {
                if (DecoratorManager == null)
                    return;

                Size = items.CountEx();
                Reset();
            }

            public void OnBeginBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                if (DecoratorManager != null)
                    AddToken(DecoratorManager.BatchUpdate(Decorator.Owner, Decorator));
            }

            public void OnEndBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
            {
                using var _ = MugenExtensions.TryLock(Collection);
                RemoveToken();
            }

            public void OnChanged(object owner, ILocker locker, IReadOnlyMetadataContext? metadata)
            {
                if (Decorator.OwnerOptional is ISynchronizable synchronizable)
                    synchronizable.UpdateLocker(locker);
            }

            protected abstract IEnumerable<object?> GetItems();

            private void Reset() => DecoratorManager!.OnReset(Decorator.Owner, Decorator, Decorator.Decorate(DecoratorManager.Decorate(Decorator.Owner, Decorator)));

            private void AddToken(ActionToken actionToken)
            {
                if (_detached)
                    actionToken.Dispose();
                else
                {
                    _tokens ??= new ListInternal<ActionToken>(2);
                    _tokens.Add(actionToken);
                }
            }

            private void RemoveToken()
            {
                if (_tokens == null || _tokens.Count == 0)
                    return;

                var item = _tokens.Items[_tokens.Count - 1];
                _tokens.RemoveAt(_tokens.Count - 1);
                item.Dispose();
            }

            bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

            void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata) => CollectionDecoratorManager.GetOrAdd((IEnumerable)owner);
        }

        private sealed class SourceFlattenCollectionItem<T> : FlattenCollectionItemBase, ICollectionChangedListener<T>
        {
            private static IEnumerable<object?>? AsObjectEnumerable(IEnumerable<T>? items) => items == null ? null : items as IEnumerable<object?> ?? items.Cast<object>();

            public void OnChanged(IReadOnlyCollection<T> collection, T item, int index, object? args)
            {
                using var _ = BatchIfNeed();
                OnChanged((ICollection)collection, item, index, args);
            }

            public void OnAdded(IReadOnlyCollection<T> collection, T item, int index)
            {
                using var _ = BatchIfNeed();
                OnAdded((ICollection)collection, item, index);
            }

            public void OnReplaced(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
            {
                using var _ = BatchIfNeed();
                OnReplaced((ICollection)collection, oldItem, newItem, index);
            }

            public void OnMoved(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
            {
                using var _ = BatchIfNeed();
                OnMoved((ICollection)collection, item, oldIndex, newIndex);
            }

            public void OnRemoved(IReadOnlyCollection<T> collection, T item, int index)
            {
                using var _ = BatchIfNeed();
                OnRemoved((ICollection)collection, item, index);
            }

            public void OnReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items) => OnReset((ICollection)collection, AsObjectEnumerable(items));

            protected override IEnumerable<object?> GetItems() => Collection.AsEnumerable();

            private ActionToken BatchIfNeed() => DecoratorManager != null && Count > 1 ? DecoratorManager.BatchUpdate(Decorator.Owner, Decorator) : default;
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