using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    internal abstract class FlattenCollectionItemBase : ILockerChangedListener<IReadOnlyObservableCollection>, IDisposableComponent<IReadOnlyObservableCollection>, IHasPriority
    {
        public object Item = null!;
        public IEnumerable Collection = null!;

        private FlattenCollectionDecorator? _decorator;
        private IWeakReference? _decoratorRef;

        protected FlattenCollectionItemBase(object item, IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak)
        {
            Initialize(item, collection, decorator, isWeak);
        }

        protected FlattenCollectionItemBase()
        {
        }

        public int Size { get; private set; }

        public int Index { get; set; }

        public int Priority => CollectionComponentPriority.BindableAdapter;

        public FlattenCollectionItemBase Initialize(object item, IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak)
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

        public void FindAllIndexOf(FlattenCollectionDecorator decorator, object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            var index = 0;
            int? originalIndex = null;
            using var _ = MugenExtensions.Lock(Collection);
            foreach (var value in GetItems())
            {
                if (Equals(item, value))
                {
                    indexes.Add((originalIndex ??= decorator.GetIndex(Index)) + index);
                    if (ignoreDuplicates)
                        return;
                }

                ++index;
            }
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
                    if (Size <= decorator.BatchThreshold)
                    {
                        if (isReplace)
                        {
                            decoratorManager.OnReplaced(source, decorator, replaceItem, item, index++);
                            isReplace = false;
                        }
                        else
                            decoratorManager.OnAdded(source, decorator, item, index++);
                    }

                    ++Size;
                }

                if (Size > decorator.BatchThreshold)
                {
                    isReset = true;
                    Reset(decoratorManager, decorator);
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
            oldItem.Detach();
            Index = oldItem.Index;
            if (oldItem.Size > decorator.BatchThreshold)
            {
                Size = GetItems().CountEx();
                Reset(decoratorManager, decorator);
            }
            else
            {
                using (var oldEnumerator = oldItem.GetItems().GetEnumerator())
                using (var newEnumerator = GetItems().GetEnumerator())
                {
                    while (true)
                    {
                        var oldHasNext = oldEnumerator.MoveNext();
                        var newHasNext = newEnumerator.MoveNext();
                        if (!newHasNext && !oldHasNext)
                            break;

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

                        if (newHasNext)
                            ++Size;
                    }
                }

                if (Size > decorator.BatchThreshold)
                    Reset(decoratorManager, decorator);
            }

            Attach(source);
        }

        public void OnRemoved(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, int index,
            out bool isReset, object? replaceItem = null, bool isReplace = false)
        {
            using var _ = MugenExtensions.Lock(Collection);
            if (Size > decorator.BatchThreshold)
            {
                isReset = true;
                Reset(decoratorManager, decorator);
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

            Detach();
        }

        public void OnMoved(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection source, int oldIndex,
            int newIndex)
        {
            using var _ = MugenExtensions.Lock(Collection);
            if (Size > decorator.BatchThreshold)
            {
                Reset(decoratorManager, decorator);
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

        public void Detach()
        {
            if (Collection is IReadOnlyObservableCollection owner)
                owner.Components.Remove(this);
        }

        public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            decoratorManager.OnChanged(decorator.Owner, decorator, item, decorator.GetIndex(Index) + index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            ++Size;
            decoratorManager.OnAdded(decorator.Owner, decorator, item, decorator.GetIndex(Index) + index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            decoratorManager.OnReplaced(decorator.Owner, decorator, oldItem, newItem, decorator.GetIndex(Index) + index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            var originalIndex = decorator.GetIndex(Index);
            decoratorManager.OnMoved(decorator.Owner, decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            --Size;
            decoratorManager.OnRemoved(decorator.Owner, decorator, item, decorator.GetIndex(Index) + index);
        }

        public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            Size = items.CountEx();
            Reset(decoratorManager, decorator);
        }

        public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (TryGetDecoratorManager(out _, out var decorator))
                decorator.Owner.GetBatchUpdateManager().BeginBatchUpdate(decorator.Owner, BatchUpdateType.Decorators);
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (TryGetDecoratorManager(out _, out var decorator))
                decorator.Owner.GetBatchUpdateManager().EndBatchUpdate(decorator.Owner, BatchUpdateType.Decorators);
        }

        public void OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            if (TryGetDecoratorManager(out _, out var decorator))
                decorator.OwnerOptional?.UpdateLocker(locker);
        }

        public void OnDisposing(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            Collection = Default.EmptyEnumerable<object?>();
            Size = 0;
            Reset(decoratorManager, decorator);
        }

        public void OnDisposed(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected internal abstract IEnumerable<object?> GetItems();

        protected bool TryGetDecoratorManager([NotNullWhen(true)] out ICollectionDecoratorManagerComponent? decoratorManager,
            [NotNullWhen(true)] out FlattenCollectionDecorator? decorator)
        {
            if (_decorator == null)
            {
                decorator = (FlattenCollectionDecorator?) _decoratorRef!.Target;
                if (decorator == null)
                {
                    if (Collection is IReadOnlyObservableCollection owner)
                        owner.Components.Remove(this);
                    decoratorManager = null;
                    return false;
                }
            }
            else
                decorator = _decorator;

            decoratorManager = decorator.DecoratorManager;
            return decoratorManager != null;
        }

        private static void Reset(ICollectionDecoratorManagerComponent decoratorManager, FlattenCollectionDecorator decorator) =>
            decoratorManager.OnReset(decorator.Owner, decorator, decorator.Decorate(decoratorManager.Decorate(decorator.Owner, decorator)));

        private void Attach(IReadOnlyObservableCollection source)
        {
            if (Collection is IReadOnlyObservableCollection owner)
                owner.Components.Add(this);

            if (Collection is ISynchronizable collectionSynchronizable)
            {
                collectionSynchronizable.UpdateLocker(source.Locker);
                source.UpdateLocker(collectionSynchronizable.Locker);
            }
        }
    }
}