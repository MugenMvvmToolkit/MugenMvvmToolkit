using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManager<T> : AttachableComponentBase<IReadOnlyObservableCollection>, ICollectionDecoratorManagerComponent, ICollectionChangedListener<T>,
        IComponentCollectionChangedListener, IHasPendingNotifications, ICollectionBatchUpdateListener, IDisposableComponent<IReadOnlyObservableCollection>, IHasPriority
    {
        private const int InitializedFlag = 1;
        private const int DirtyFlag = 1 << 1;
        private const int BatchSourceFlag = 1 << 2;
        private const int BatchFlag = 1 << 3;
        private const int ChangeOperationFlag = 1 << 4;
        private const int NonChangeOperationFlag = 1 << 5;
        private const int OperationCanceledFlag = 1 << 6;
        private const int DisposedFlag = 1 << 7;

        private const int EmptyIndex = -1;
        private const int InvalidDecoratorIndex = -1;
        private readonly HashSet<(object?, object?, int)> _pendingChangedItems;

        private Dictionary<ICollectionDecorator, DecoratorItems>? _decoratorCache;
        private List<object?>? _sourceSnapshot;
        private Timer? _changedItemsTimer;
        private int _version;
        private int _updatingIndex;
        private int _flags;
        private int _flattenListenerCount;
        private int? _raiseItemChangedDelay;
        private int? _raiseItemChangedResetThreshold;
        private bool? _raiseItemChangedCheckDuplicates;

        [Preserve]
        public CollectionDecoratorManager() : this(CollectionComponentPriority.DecoratorManager)
        {
        }

        public CollectionDecoratorManager(int priority)
        {
            Priority = priority;
            _updatingIndex = EmptyIndex;
            _pendingChangedItems = new HashSet<(object?, object?, int)>();
        }

        public bool RaiseItemChangedCheckDuplicates
        {
            get => _raiseItemChangedCheckDuplicates.GetValueOrDefault(CollectionMetadata.RaiseItemChangedCheckDuplicates);
            set => _raiseItemChangedCheckDuplicates = value;
        }

        public int RaiseItemChangedDelay
        {
            get => _raiseItemChangedDelay.GetValueOrDefault(CollectionMetadata.RaiseItemChangedDelay);
            set => _raiseItemChangedDelay = value;
        }

        public int? RaiseItemChangedResetThreshold
        {
            get => _raiseItemChangedResetThreshold.GetValueOrDefault(CollectionMetadata.RaiseItemChangedResetThreshold);
            set => _raiseItemChangedResetThreshold = value;
        }

        public int Priority { get; init; }

        public IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator = null)
        {
            using var _ = decorator == null || !CheckFlag(InitializedFlag) ? collection.Lock() : default;
            if (!CheckFlag(InitializedFlag))
                Reset(null, true, false);

            var items = GetSource(collection, false);
            var decorators = GetDecorators(collection, decorator, out var index, true);
            if (index == InvalidDecoratorIndex)
                yield break;

            int startIndex;
            DecoratorItems? decoratorItems = null;
            if (_decoratorCache != null && _decoratorCache.Count != 0)
            {
                foreach (var item in _decoratorCache)
                {
                    var value = item.Value;
                    if (value.Index < index && (decoratorItems == null || value.Index > decoratorItems.Index))
                        decoratorItems = value;
                }

                if (decoratorItems == null)
                    startIndex = 0;
                else
                {
                    startIndex = decoratorItems.Index + 1;
                    items = decoratorItems;
                }
            }
            else
                startIndex = 0;

            for (var i = startIndex; i < index; i++)
                items = decorators[i].Decorate(collection, items);

            foreach (var item in items)
                yield return item;
        }

        public void RaiseItemChanged(IReadOnlyObservableCollection collection, object? item, object? args)
        {
            ActionToken token = default;
            try
            {
                if (CheckFlag(BatchFlag) || _flattenListenerCount > 0 || !collection.TryLock(0, out token) || CheckFlag(BatchFlag) || _flattenListenerCount > 0)
                {
                    token.Dispose();
                    lock (_pendingChangedItems)
                    {
                        _pendingChangedItems.Add((item, args, _version));
                        _changedItemsTimer ??= WeakTimer.Get(this, manager => manager.RaisePendingChanges());
                        _changedItemsTimer.SafeChange(RaiseItemChangedDelay, Timeout.Infinite);
                        return;
                    }
                }

                RaiseItemChangedInternal(collection, item, args);
            }
            finally
            {
                token.Dispose();
            }
        }

        public void OnChanged(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index, object? args)
        {
            if (!CanUpdate())
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!token.OnUpdate(i, true) || !decorators[i].OnChanged(collection, ref item, ref index, ref args) || !token.CanContinue(collection, item, args))
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnChanged(collection, item, index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            if (!CanUpdate())
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            GetItems(decorator)?.Insert(index, item);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (!token.OnUpdate(i, false) || !decorator.OnAdded(collection, ref item, ref index) || !token.CanContinue())
                    return;

                GetItems(decorator)?.Insert(index, item);
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnAdded(collection, item, index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index)
        {
            if (!CanUpdate())
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            GetItems(decorator)?.Set(index, newItem);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (!token.OnUpdate(i, false) || !decorator.OnReplaced(collection, ref oldItem, ref newItem, ref index) || ReferenceEquals(oldItem, newItem) ||
                    !token.CanContinue())
                    return;

                GetItems(decorator)?.Set(index, newItem);
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnReplaced(collection, oldItem, newItem, index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex)
        {
            if (!CanUpdate())
                return;
            if (oldIndex == newIndex)
                return;

            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            GetItems(decorator)?.Move(oldIndex, newIndex);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (!token.OnUpdate(i, false) || !decorator.OnMoved(collection, ref item, ref oldIndex, ref newIndex) || oldIndex == newIndex || !token.CanContinue())
                    return;

                GetItems(decorator)?.Move(oldIndex, newIndex);
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnMoved(collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            if (!CanUpdate())
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            GetItems(decorator)?.RemoveAt(index);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (!token.OnUpdate(i, false) || !decorator.OnRemoved(collection, ref item, ref index) || !token.CanContinue())
                    return;

                GetItems(decorator)?.RemoveAt(index);
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnRemoved(collection, item, index);
        }

        public void OnReset(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items) => OnReset(collection, decorator, items, false);

        protected override void OnAttaching(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttaching(owner, metadata);
            owner.Components.AddComponent(this);
            foreach (var decorator in owner.GetComponents<ICollectionDecorator>())
                AddToCacheIfNeed(owner, decorator);
            UpdateDecoratorIndexes(owner);
            _flattenListenerCount = owner.GetComponents<IFlattenCollectionListener>().Count;
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            owner.Components.RemoveComponent(this);
        }

        private static void FindAllIndexOf(IReadOnlyObservableCollection collection, ICollectionDecorator decorator, IEnumerable<object?> items, object? item,
            bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (!decorator.HasAdditionalItems(collection))
                return;

            indexes.Clear();
            if (!decorator.TryGetIndexes(collection, items, item, ignoreDuplicates, ref indexes))
                items.FindAllIndexOf(item, ignoreDuplicates, ref indexes);
        }

        private static ItemOrArray<ICollectionDecorator> GetDecorators(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, out int index,
            bool isLengthDefault = false)
        {
            var components = collection.GetComponents<ICollectionDecorator>();
            index = isLengthDefault ? components.Count : 0;
            if (decorator == null)
                return components;

            var found = false;
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == decorator)
                {
                    found = true;
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            if (!found)
            {
                index = InvalidDecoratorIndex;
                return default;
            }

            return components;
        }

        private DecoratorItems? GetItems(ICollectionDecorator? decorator)
        {
            if (decorator != null && _decoratorCache != null && _decoratorCache.TryGetValue(decorator, out var values))
                return values;
            return null;
        }

        private void RaisePendingChanges()
        {
            var collection = OwnerOptional;
            ItemOrArray<(object?, object?, int)> items;
            bool reset;
            lock (_pendingChangedItems)
            {
                var count = _pendingChangedItems.Count;
                if (count == 0)
                    return;

                if (collection == null)
                {
                    _pendingChangedItems.Clear();
                    return;
                }

                if (count >= RaiseItemChangedResetThreshold)
                {
                    reset = true;
                    items = default;
                }
                else
                {
                    reset = false;
                    items = ItemOrArray.Get<(object?, object?, int)>(count);
                    var index = 0;
                    foreach (var item in _pendingChangedItems)
                        items.SetAt(index++, item);
                }

                _pendingChangedItems.Clear();
            }

            using var _ = collection.Lock();
            if (reset)
                Reset(null, false, false);
            else
            {
                foreach (var item in items)
                {
                    if (item.Item3 == _version)
                        RaiseItemChangedInternal(collection, item.Item1, item.Item2);
                }
            }
        }

        private void RaiseItemChangedInternal(IReadOnlyObservableCollection collection, object? item, object? args)
        {
            if (!CanUpdate())
                return;

            var indexes = new ItemOrListEditor<int>();
            var items = GetSource(collection, false);
            items.FindAllIndexOf(item, !RaiseItemChangedCheckDuplicates, ref indexes);

            if (indexes.Count != 0)
            {
                OnChanged(collection, collection.GetComponents<ICollectionDecorator>(), 0, item, indexes, args);
                return;
            }

            var decorators = collection.GetComponents<ICollectionDecorator>();
            for (var i = 0; i < decorators.Count; i++)
            {
                var decorator = decorators[i];
                items = GetItems(decorator) ?? decorator.Decorate(collection, items);
                FindAllIndexOf(collection, decorator, items, item, !RaiseItemChangedCheckDuplicates, ref indexes);
                if (indexes.Count != 0)
                {
                    OnChanged(collection, decorators, i + 1, item, indexes, args);
                    return;
                }
            }
        }

        private void OnReset(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items, bool force)
        {
            if (!force && !CanUpdate())
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            if (decorator == null)
            {
                ++_version;
                _updatingIndex = EmptyIndex;
            }

            GetItems(decorator)?.Reset(items);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (!token.OnUpdate(i, false) || !decorator.OnReset(collection, ref items) || !token.CanContinue())
                    return;

                GetItems(decorator)?.Reset(items);
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnReset(collection, items);
        }

        private void OnChanged(IReadOnlyObservableCollection collection, ItemOrArray<ICollectionDecorator> decorators, int startIndex, object? item,
            ItemOrIReadOnlyList<int> indexes, object? args)
        {
            using var token = BatchUpdate(collection);
            foreach (var currentIndex in indexes)
            {
                var index = currentIndex;
                for (var i = startIndex; i < decorators.Count; i++)
                {
                    if (!token.OnUpdate(i, true) || !decorators[i].OnChanged(collection, ref item, ref index, ref args) || !token.CanContinue(collection, item, args))
                        return;
                }

                collection.GetComponents<IDecoratedCollectionChangedListener>().OnChanged(collection, item, index, args);
            }
        }

        private UpdateOperationToken BatchUpdate(IReadOnlyObservableCollection collection)
        {
            if (CheckFlag(BatchFlag))
                return new UpdateOperationToken(default, this);
            SetFlag(BatchFlag);
            return new UpdateOperationToken(collection.BatchUpdateDecorators(collection.GetBatchUpdateManager()), this);
        }

        private void Reset(object? component, bool force, bool remove)
        {
            var collection = OwnerOptional;
            if (collection == null)
                return;

            if (force || CanReset(collection, component))
            {
                if (!CheckFlag(InitializedFlag))
                {
                    SetFlag(InitializedFlag);
                    OnReset(collection, null, GetSource(collection, true), true);
                    return;
                }

                if (!remove && component is ICollectionDecorator decorator)
                {
                    var items = Decorate(collection, decorator);
                    if (decorator.OnReset(collection, ref items) && decorator is not IListenerCollectionDecorator)
                        OnReset(collection, decorator, items, true);
                }
                else
                    OnReset(collection, null, GetSource(collection, true), true);
            }
        }

        private IEnumerable<object?> GetSource(IReadOnlyObservableCollection collection, bool force)
        {
            if (CheckFlag(BatchSourceFlag) && !CheckFlag(DisposedFlag))
            {
                if (force)
                {
                    ClearFlag(DirtyFlag);
                    InitializeSnapshot(collection);
                }

                return _sourceSnapshot ?? Default.EmptyEnumerable<object?>();
            }

            return collection.AsEnumerable();
        }

        private bool CanUpdate()
        {
            if (!CheckFlag(InitializedFlag))
                return false;
            if (!CheckFlag(BatchSourceFlag))
                return true;
            SetFlag(DirtyFlag);
            return false;
        }

        private bool CanReset(IReadOnlyObservableCollection collection, object? component)
        {
            if (CheckFlag(InitializedFlag))
                return component is null or ICollectionDecorator;
            return component is IListenerCollectionDecorator or IDecoratedCollectionChangedListener ||
                   component is ICollectionDecorator collectionDecorator && collectionDecorator.IsLazy(collection);
        }

        private void InitializeSnapshot(IReadOnlyObservableCollection collection)
        {
            if (collection.Count <= 0)
                return;
            if (_sourceSnapshot == null)
                _sourceSnapshot = new List<object?>(collection.AsEnumerable());
            else
            {
                _sourceSnapshot.Clear();
                _sourceSnapshot.AddRange(collection.AsEnumerable());
            }
        }

        private void UpdateDecoratorIndexes(IReadOnlyObservableCollection collection)
        {
            if (_decoratorCache == null || _decoratorCache.Count == 0)
                return;

            var updatedCount = 0;
            var decorators = collection.GetComponents<ICollectionDecorator>();
            for (var i = 0; i < decorators.Count; i++)
            {
                if (_decoratorCache.TryGetValue(decorators[i], out var value))
                {
                    value.Index = i;
                    if (++updatedCount == _decoratorCache.Count)
                        break;
                }
            }
        }

        private void AddToCacheIfNeed(IReadOnlyObservableCollection collection, ICollectionDecorator decorator)
        {
            if (decorator.IsCacheRequired(collection))
            {
                _decoratorCache ??= new Dictionary<ICollectionDecorator, DecoratorItems>(InternalEqualityComparer.Reference);
                _decoratorCache[decorator] = new DecoratorItems();
            }
        }

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Source)
                return;
            SetFlag(BatchSourceFlag);
            InitializeSnapshot(collection);
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Source)
                return;
            ClearFlag(BatchSourceFlag);
            _sourceSnapshot?.Clear();
            if (CheckFlag(DirtyFlag))
            {
                ClearFlag(DirtyFlag);
                if (!CheckFlag(DisposedFlag))
                    Reset(null, true, false);
            }
        }

        void ICollectionChangedListener<T>.OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAdded(collection, null, item, index);

        void ICollectionChangedListener<T>.OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) =>
            OnReplaced(collection, null, oldItem, newItem, index);

        void ICollectionChangedListener<T>.OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) =>
            OnMoved(collection, null, item, oldIndex, newIndex);

        void ICollectionChangedListener<T>.OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) =>
            OnRemoved(collection, null, item, index);

        void ICollectionChangedListener<T>.OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items)
        {
            if (items == null)
                OnReset(collection, null, null);
            else
                OnReset(collection, null, items as IEnumerable<object?> ?? items.Cast<object?>());
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IFlattenCollectionListener)
                ++_flattenListenerCount;
            if (component is ICollectionDecorator decorator)
            {
                var owner = (IReadOnlyObservableCollection) collection.Owner;
                AddToCacheIfNeed(owner, decorator);
                UpdateDecoratorIndexes(owner);
            }

            Reset(component, false, false);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (CheckFlag(DisposedFlag))
                return;
            if (component is IFlattenCollectionListener)
                --_flattenListenerCount;
            if (component is ICollectionDecorator decorator)
            {
                _decoratorCache?.Remove(decorator);
                UpdateDecoratorIndexes((IReadOnlyObservableCollection) collection.Owner);
            }

            Reset(component, false, true);
        }

        void IDisposableComponent<IReadOnlyObservableCollection>.OnDisposing(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            SetFlag(DisposedFlag | BatchSourceFlag);
            owner.RemoveComponents<IDecoratedCollectionChangedListener>(metadata);
        }

        void IDisposableComponent<IReadOnlyObservableCollection>.OnDisposed(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            var decorators = owner.GetComponents<ICollectionDecorator>();
            for (var i = decorators.Count - 1; i >= 0; i--)
                owner.Components.Remove(decorators[i], metadata);
            _changedItemsTimer?.Dispose();
            _decoratorCache?.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckFlag(int flag) => (_flags & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFlag(int flag) => _flags |= flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearFlag(int flag) => _flags &= ~flag;

        void IHasPendingNotifications.Raise(IReadOnlyMetadataContext? metadata) => RaisePendingChanges();

        [StructLayout(LayoutKind.Auto)]
        private struct UpdateOperationToken : IDisposable
        {
            private ActionToken _token;
            private readonly CollectionDecoratorManager<T> _decoratorManager;
            private readonly int _oldIndex;
            private readonly int _oldFlags;
            private readonly int _version;

            public UpdateOperationToken(ActionToken token, CollectionDecoratorManager<T> decoratorManager)
            {
                _token = token;
                _decoratorManager = decoratorManager;
                _oldIndex = decoratorManager._updatingIndex;
                _oldFlags = 0;
                _version = decoratorManager._version;
                if (decoratorManager.CheckFlag(ChangeOperationFlag))
                    _oldFlags |= ChangeOperationFlag;
                if (decoratorManager.CheckFlag(NonChangeOperationFlag))
                    _oldFlags |= NonChangeOperationFlag;
            }

            public readonly bool OnUpdate(int startIndex, bool isChange)
            {
                if (startIndex <= _decoratorManager._updatingIndex)
                {
                    if (!_decoratorManager.CheckFlag(NonChangeOperationFlag))
                    {
                        _decoratorManager.SetFlag(OperationCanceledFlag);
                        return false;
                    }

                    _decoratorManager.Reset(null, false, false);
                    return false;
                }

                _decoratorManager._updatingIndex = startIndex;
                _decoratorManager.SetFlag(isChange ? ChangeOperationFlag : NonChangeOperationFlag);
                return true;
            }

            public readonly bool CanContinue(IReadOnlyObservableCollection collection, object? item, object? args)
            {
                if (_decoratorManager._version != _version)
                    return false;

                if (!_decoratorManager.CheckFlag(OperationCanceledFlag))
                    return true;

                _decoratorManager.RaiseItemChanged(collection, item, args);
                return false;
            }

            public readonly bool CanContinue() => _decoratorManager._version == _version;

            public void Dispose()
            {
                if (_token.IsEmpty)
                {
                    _decoratorManager._updatingIndex = _oldIndex;
                    _decoratorManager.ClearFlag(ChangeOperationFlag | NonChangeOperationFlag);
                    _decoratorManager._flags |= _oldFlags;
                }
                else
                {
                    _decoratorManager.ClearFlag(ChangeOperationFlag | NonChangeOperationFlag | OperationCanceledFlag | BatchFlag);
                    _decoratorManager._updatingIndex = EmptyIndex;
                    _token.Dispose();
                }
            }
        }

        private sealed class DecoratorItems : List<object?>
        {
            public int Index;

            public void Set(int index, object? item) => this[index] = item;
        }
    }
}