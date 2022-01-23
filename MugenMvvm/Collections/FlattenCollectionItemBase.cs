using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MugenMvvm.Collections.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal abstract class FlattenCollectionItemBase : IndexMapAware, ILockerChangedListener<IReadOnlyObservableCollection>, IDisposableComponent<IReadOnlyObservableCollection>,
        IHasPriority
    {
        public object? Item;
        public IEnumerable Collection = null!;
        public int Size;

        private FlattenCollectionDecorator? _decorator;
        private IWeakReference? _decoratorRef;
        private bool _initialized;
        private bool _isInBatch;
        private bool _isDirty;
        private bool _isCleared;

        protected FlattenCollectionItemBase(object? item, IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak)
        {
            Initialize(item, collection, decorator, isWeak);
        }

        protected FlattenCollectionItemBase()
        {
        }

        public int Priority => CollectionComponentPriority.BindableAdapter;

        public FlattenItemInfo ToFlattenItemInfo() => new(Collection, this is FlattenDecoratedCollectionItem);

        public virtual void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (!_initialized)
                _isInBatch = true;
            if (TryGetDecoratorManager(out _, out _, out var owner))
                owner.GetBatchUpdateManager().BeginBatchUpdate(owner, BatchUpdateType.Decorators);
        }

        public virtual void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
                return;

            ActionToken token = default;
            try
            {
                if (!owner.TryLock(0, out token)) //possible deadlock
                {
                    Task.Run(() =>
                    {
                        using (collection.Lock())
                        {
                            OnEndBatchUpdate(collection, batchUpdateType);
                        }
                    });
                    return;
                }

                if (_isInBatch)
                {
                    _isInBatch = false;
                    if (_isDirty)
                    {
                        _isDirty = false;
                        Size = GetItems().CountEx();
                        using (owner.Lock())
                        {
                            Reset(decoratorManager, decorator, owner);
                        }
                    }
                }

                owner.GetBatchUpdateManager().EndBatchUpdate(owner, BatchUpdateType.Decorators);
            }
            finally
            {
                token.Dispose();
            }
        }

        public FlattenCollectionItemBase Initialize(object? item, IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak)
        {
            Item = item;
            Collection = collection;
            if (isWeak)
                _decoratorRef = decorator.WeakReference;
            else
                _decorator = decorator;
            return this;
        }

        public void UpdateLocker(ILocker locker)
        {
            if (Collection is ISynchronizable synchronizable)
                synchronizable.UpdateLocker(locker);
        }

        public void OnAdded(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, int index,
            bool notify, bool isRecycled, out bool isReset, object? replaceItem = null, bool isReplace = false)
        {
            using var _ = MugenExtensions.Lock(Collection);
            isReset = false;
            if (notify)
            {
                foreach (var item in GetItems())
                {
                    if (Size++ <= decorator.BatchThreshold)
                    {
                        if (isReplace)
                        {
                            decoratorManager.OnReplaced(source, decorator, replaceItem, item, index++);
                            isReplace = false;
                        }
                        else
                            decoratorManager.OnAdded(source, decorator, item, index++);
                    }
                }

                if (Size > decorator.BatchThreshold)
                {
                    isReset = true;
                    Reset(decoratorManager, decorator, source);
                }
                else if (isReplace)
                    decoratorManager.OnRemoved(source, decorator, replaceItem, index);
            }
            else if (!isRecycled)
                Size = GetItems().CountEx();

            if (!isRecycled)
                Attach(source);
        }

        public void OnReplaced(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, int index,
            FlattenCollectionItemBase oldItem)
        {
            using var _ = MugenExtensions.Lock(oldItem.Collection);
            using var __ = MugenExtensions.Lock(Collection);
            oldItem.Detach(null);
            Index = oldItem.Index;
            var oldSize = oldItem.Size;
            if (oldItem.Size > decorator.BatchThreshold)
            {
                Size = GetItems().CountEx();
                Reset(decoratorManager, decorator, source);
            }
            else
            {
                int count = 0;
                using (var oldEnumerator = oldItem.GetItems().GetEnumerator())
                using (var newEnumerator = GetItems().GetEnumerator())
                {
                    while (true)
                    {
                        var oldHasNext = oldSize > count && oldEnumerator.MoveNext();
                        var newHasNext = newEnumerator.MoveNext();
                        if (!newHasNext && !oldHasNext)
                            break;

                        if (newHasNext)
                            ++Size;

                        if (Size <= decorator.BatchThreshold)
                        {
                            if (oldHasNext && newHasNext)
                                decoratorManager.OnReplaced(source, decorator, oldEnumerator.Current, newEnumerator.Current, index++);
                            else if (newHasNext)
                                decoratorManager.OnAdded(source, decorator, newEnumerator.Current, index++);
                            else
                                decoratorManager.OnRemoved(source, decorator, oldEnumerator.Current, index);
                        }
                        else if (!newHasNext)
                            break;

                        ++count;
                    }
                }

                if (Size > decorator.BatchThreshold)
                    Reset(decoratorManager, decorator, source);
            }

            decorator.OnDetached(oldItem);
            Attach(source);
        }

        public void OnRemoved(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, int index,
            out bool isReset, object? replaceItem = null, bool isReplace = false)
        {
            using var _ = MugenExtensions.Lock(Collection);
            if (Size > decorator.BatchThreshold)
            {
                isReset = true;
                Reset(decoratorManager, decorator, source);
            }
            else
            {
                isReset = false;
                if (Size == 0)
                {
                    if (isReplace)
                        decoratorManager.OnAdded(source, decorator, replaceItem, index);
                }
                else
                {
                    if (isReplace)
                    {
                        ++index;
                        foreach (var item in GetItems())
                        {
                            if (isReplace)
                            {
                                decoratorManager.OnReplaced(source, decorator, item, replaceItem, index - 1);
                                isReplace = false;
                            }
                            else
                                decoratorManager.OnRemoved(source, decorator, item, index);
                        }
                    }
                    else
                    {
                        foreach (var item in GetItems())
                            decoratorManager.OnRemoved(source, decorator, item, index);
                    }
                }
            }

            Detach(decorator);
        }

        public void OnMoved(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, int oldIndex,
            int newIndex)
        {
            using var _ = MugenExtensions.Lock(Collection);
            if (Size > decorator.BatchThreshold)
            {
                Reset(decoratorManager, decorator, source);
                return;
            }

            if (Size == 0)
                return;

            var removeIndex = 0;
            var addIndex = 0;
            if (oldIndex < newIndex)
                addIndex += Size - 1;
            foreach (var item in GetItems())
            {
                decoratorManager.OnMoved(source, decorator, item, oldIndex + removeIndex, newIndex + addIndex);
                if (oldIndex > newIndex)
                {
                    ++removeIndex;
                    ++addIndex;
                }
            }
        }

        public void Detach(FlattenCollectionDecorator? decorator)
        {
            if (Collection is IReadOnlyObservableCollection owner)
                owner.Components.Remove(this);
            decorator?.OnDetached(this);
        }

        public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            if (_isInBatch || !TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
            {
                _isDirty = true;
                return;
            }

            decoratorManager.OnChanged(owner, decorator, item, decorator.GetIndex(Index) + index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (_isInBatch || !TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
            {
                _isDirty = true;
                return;
            }

            ++Size;
            decoratorManager.OnAdded(owner, decorator, item, decorator.GetIndex(Index) + index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            if (_isInBatch || !TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
            {
                _isDirty = true;
                return;
            }

            decoratorManager.OnReplaced(owner, decorator, oldItem, newItem, decorator.GetIndex(Index) + index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            if (_isInBatch || !TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
            {
                _isDirty = true;
                return;
            }

            var originalIndex = decorator.GetIndex(Index);
            decoratorManager.OnMoved(owner, decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (_isInBatch || !TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
            {
                _isDirty = true;
                return;
            }

            --Size;
            decoratorManager.OnRemoved(owner, decorator, item, decorator.GetIndex(Index) + index);
        }

        public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items)
        {
            if (_isInBatch || !TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner))
            {
                _isDirty = true;
                return;
            }

            Size = items.CountEx();
            Reset(decoratorManager, decorator, owner);
        }

        public void OnDisposing(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator, out var decoratorOwner))
                return;

            Collection = Default.Enumerable<object?>();
            Size = 0;
            Reset(decoratorManager, decorator, decoratorOwner);
        }

        public void OnDisposed(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
        }

        public void OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            if (TryGetDecoratorManager(out _, out _, out var decoratorOwner))
                decoratorOwner.UpdateLocker(locker);
        }

        protected internal abstract IEnumerable<object?> GetItems();

        protected static void Reset(ICollectionDecoratorManagerComponent decoratorManager, FlattenCollectionDecorator decorator, IReadOnlyObservableCollection owner) =>
            decoratorManager.OnReset(owner, decorator, decorator.Decorate(decoratorManager.Decorate(owner, decorator, false)));

        protected bool TryGetDecoratorManager([NotNullWhen(true)] out ICollectionDecoratorManagerComponent? decoratorManager,
            [NotNullWhen(true)] out FlattenCollectionDecorator? decorator, [NotNullWhen(true)] out IReadOnlyObservableCollection? decoratorOwner)
        {
            if (_decorator == null)
            {
                decorator = (FlattenCollectionDecorator?) _decoratorRef!.Target;
                if (decorator == null)
                {
                    if (!_isCleared)
                    {
                        _isCleared = true;
                        if (Collection is IReadOnlyObservableCollection owner)
                            owner.Components.Remove(this);
                    }

                    decoratorOwner = null;
                    decoratorManager = null;
                    return false;
                }
            }
            else
                decorator = _decorator;

            decoratorManager = decorator.DecoratorManager;
            decoratorOwner = decorator.OwnerOptional;
            return decoratorManager != null && decoratorOwner != null;
        }

        private void Attach(IReadOnlyObservableCollection source)
        {
            if (Collection is IReadOnlyObservableCollection owner)
                owner.Components.Add(this);

            if (Collection is ISynchronizable collectionSynchronizable)
            {
                collectionSynchronizable.UpdateLocker(source.Locker);
                source.UpdateLocker(collectionSynchronizable.Locker);
            }

            _initialized = true;
        }
    }
}