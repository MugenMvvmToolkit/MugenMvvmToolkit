﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManager<T> : AttachableComponentBase<IReadOnlyObservableCollection>, ICollectionDecoratorManagerComponent, ICollectionChangedListener<T>,
        IComponentCollectionChangedListener, ICollectionBatchUpdateListener, IDisposableComponent<IReadOnlyObservableCollection>, IHasPriority
    {
        private const int InvalidDecoratorIndex = -1;
        private readonly HashSet<(object?, object?)> _pendingChangedItems;
        private int _version;
        private Dictionary<ICollectionDecorator, DecoratorItems>? _decoratorCache;
        private bool _isDirty;
        private bool _isInBatchSource;
        private List<object?>? _sourceSnapshot;
        private Timer? _changedItemsTimer;
        private bool _isInBatch;
        private bool _isDisposed;
        private bool _isInitialized;
        private int? _raiseItemChangedDelay;
        private int? _raiseItemChangedResetThreshold;
        private bool? _raiseItemChangedAsync;
        private bool? _raiseItemChangedCheckDuplicates;
        private int? _raiseItemChangedLockTimeout;

        [Preserve]
        public CollectionDecoratorManager() : this(CollectionComponentPriority.DecoratorManager)
        {
        }

        public CollectionDecoratorManager(int priority)
        {
            Priority = priority;
            _pendingChangedItems = new HashSet<(object?, object?)>(InternalEqualityComparer.ValueTupleReference);
        }

        public int RaiseItemChangedLockTimeout
        {
            get => _raiseItemChangedLockTimeout.GetValueOrDefault(CollectionMetadata.RaiseItemChangedLockTimeout);
            set => _raiseItemChangedLockTimeout = value;
        }

        public bool RaiseItemChangedCheckDuplicates
        {
            get => _raiseItemChangedCheckDuplicates.GetValueOrDefault(CollectionMetadata.RaiseItemChangedCheckDuplicates);
            set => _raiseItemChangedCheckDuplicates = value;
        }

        public bool RaiseItemChangedAsync
        {
            get => _raiseItemChangedAsync.GetValueOrDefault(CollectionMetadata.RaiseItemChangedAsync);
            set => _raiseItemChangedAsync = value;
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
            using var _ = decorator == null || !_isInitialized ? collection.Lock() : default;
            if (!_isInitialized)
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
                if (RaiseItemChangedAsync || !collection.TryLock(RaiseItemChangedLockTimeout, out token))
                {
                    lock (_pendingChangedItems)
                    {
                        _pendingChangedItems.Add((item, args));
                        _changedItemsTimer ??= WeakTimer.Get(this, manager => manager.RaiseItemChangedTimer());
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

            var version = _version;
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (version != _version || !decorators[i].OnChanged(collection, ref item, ref index, ref args))
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

            var version = _version;
            GetItems(decorator)?.Insert(index, item);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (version != _version || !decorator.OnAdded(collection, ref item, ref index))
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

            var version = _version;
            GetItems(decorator)?.Set(index, newItem);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (version != _version || !decorator.OnReplaced(collection, ref oldItem, ref newItem, ref index) || ReferenceEquals(oldItem, newItem))
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

            var version = _version;
            GetItems(decorator)?.Move(oldIndex, newIndex);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (version != _version || !decorator.OnMoved(collection, ref item, ref oldIndex, ref newIndex) || oldIndex == newIndex)
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

            var version = _version;
            GetItems(decorator)?.RemoveAt(index);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (version != _version || !decorator.OnRemoved(collection, ref item, ref index))
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

        private void RaiseItemChangedTimer()
        {
            var collection = OwnerOptional;
            ItemOrArray<(object?, object?)> items;
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
                    items = ItemOrArray.Get<(object?, object?)>(count);
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
                    RaiseItemChangedInternal(collection, item.Item1, item.Item2);
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

            var version = decorator == null ? ++_version : _version;
            GetItems(decorator)?.Reset(items);
            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                decorator = decorators[i];
                if (version != _version || !decorator.OnReset(collection, ref items))
                    return;

                GetItems(decorator)?.Reset(items);
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnReset(collection, items);
        }

        private void OnChanged(IReadOnlyObservableCollection collection, ItemOrArray<ICollectionDecorator> decorators, int startIndex, object? item,
            ItemOrIReadOnlyList<int> indexes, object? args)
        {
            var version = _version;
            using var token = BatchUpdate(collection);
            foreach (var currentIndex in indexes)
            {
                var index = currentIndex;
                for (var i = startIndex; i < decorators.Count; i++)
                {
                    if (version != _version || !decorators[i].OnChanged(collection, ref item, ref index, ref args))
                        return;
                }

                collection.GetComponents<IDecoratedCollectionChangedListener>().OnChanged(collection, item, index, args);
            }
        }

        private BatchUpdateToken BatchUpdate(IReadOnlyObservableCollection collection)
        {
            if (_isInBatch)
                return default;
            _isInBatch = true;
            return new BatchUpdateToken(collection.BatchUpdateDecorators(collection.GetBatchUpdateManager()), this);
        }

        private void Reset(object? component, bool force, bool remove)
        {
            var collection = OwnerOptional;
            if (collection == null)
                return;

            if (force || CanReset(collection, component))
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;
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
            if (_isInBatchSource && !_isDisposed)
            {
                if (force)
                {
                    _isDirty = false;
                    InitializeSnapshot(collection);
                }

                return _sourceSnapshot ?? Default.EmptyEnumerable<object?>();
            }

            return collection.AsEnumerable();
        }

        private bool CanUpdate()
        {
            if (!_isInitialized)
                return false;
            if (!_isInBatchSource)
                return true;
            _isDirty = true;
            return false;
        }

        private bool CanReset(IReadOnlyObservableCollection collection, object? component)
        {
            if (_isInitialized)
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

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Source)
                return;
            _isInBatchSource = true;
            InitializeSnapshot(collection);
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Source)
                return;
            _isInBatchSource = false;
            _sourceSnapshot?.Clear();
            if (_isDirty)
            {
                _isDirty = false;
                if (!_isDisposed)
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
            if (component is ICollectionDecorator decorator)
            {
                var owner = (IReadOnlyObservableCollection) collection.Owner;
                if (decorator.IsCacheRequired(owner))
                {
                    _decoratorCache ??= new Dictionary<ICollectionDecorator, DecoratorItems>(InternalEqualityComparer.Reference);
                    _decoratorCache[decorator] = new DecoratorItems();
                }

                UpdateDecoratorIndexes(owner);
            }

            Reset(component, false, false);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!_isDisposed)
            {
                if (component is ICollectionDecorator decorator)
                {
                    _decoratorCache?.Remove(decorator);
                    UpdateDecoratorIndexes((IReadOnlyObservableCollection) collection.Owner);
                }

                Reset(component, false, true);
            }
        }

        void IDisposableComponent<IReadOnlyObservableCollection>.OnDisposing(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            _isDisposed = true;
            _isInBatchSource = true;
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

        [StructLayout(LayoutKind.Auto)]
        private readonly struct BatchUpdateToken : IDisposable
        {
            private readonly ActionToken _token;
            private readonly CollectionDecoratorManager<T>? _decoratorManager;

            public BatchUpdateToken(ActionToken token, CollectionDecoratorManager<T> decoratorManager)
            {
                _token = token;
                _decoratorManager = decoratorManager;
            }

            public void Dispose()
            {
                if (_decoratorManager == null)
                    return;
                _decoratorManager._isInBatch = false;
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                _token.Dispose();
            }
        }

        private sealed class DecoratorItems : List<object?>
        {
            public int Index;

            public void Set(int index, object? item) => this[index] = item;
        }
    }
}