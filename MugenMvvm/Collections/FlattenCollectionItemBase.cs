using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal abstract class FlattenCollectionItemBase : ILockerChangedListener<IReadOnlyObservableCollection>, IHasPriority
    {
        internal IEnumerable Collection = null!;

        private FlattenCollectionDecorator? _decorator;
        private IWeakReference? _decoratorRef;
        private ListInternal<ActionToken> _tokens;
        internal ListInternal<int> Indexes;
        private bool _detached;

        protected FlattenCollectionItemBase(IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak)
        {
            Initialize(collection, decorator, isWeak);
            Indexes = new ListInternal<int>(1);
        }

        protected FlattenCollectionItemBase()
        {
            Indexes = new ListInternal<int>(1);
        }

        public int Size { get; private set; }

        public int Priority => CollectionComponentPriority.BindableAdapter;

        public FlattenCollectionItemBase Initialize(IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak)
        {
            if (isWeak)
                _decoratorRef = decorator.WeakReference;
            else
                _decorator = decorator;
            Collection = collection;
            return this;
        }

        public void UpdateLocker(ILocker locker)
        {
            if (Collection is ISynchronizable synchronizable)
                synchronizable.UpdateLocker(locker);
        }

        public void FindAllIndexOf(FlattenCollectionDecorator decorator, object? item, ref ItemOrListEditor<int> indexes)
        {
            if (Indexes.Count == 0)
                return;

            int index = 0;
            using var _ = MugenExtensions.TryLock(Collection);
            foreach (var value in GetItems())
            {
                if (Equals(item, value))
                {
                    for (int i = 0; i < Indexes.Count; i++)
                        indexes.Add(decorator.GetIndex(Indexes.Items[i]) + index);
                }

                ++index;
            }
        }

        public void OnAdded(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, object source, int originalIndex, int index, bool notify,
            bool isRecycled, out bool isReset)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            isReset = false;
            if (notify)
            {
                Size = GetItems().Count();
                if (Size > decorator.BatchThreshold)
                {
                    isReset = true;
                    Reset(decoratorManager, decorator);
                }
                else
                {
                    foreach (var item in GetItems())
                        decoratorManager.OnAdded(decorator.Owner, decorator, item, index++);
                }
            }
            else if (Size == 0)
                Size = GetItems().CountEx();

            Indexes.AddOrdered(originalIndex, Comparer<int>.Default);
            if (!isRecycled)
            {
                if (Collection is IReadOnlyObservableCollection owner)
                    owner.Components.Add(this);

                if (Collection is ISynchronizable collectionSynchronizable && source is ISynchronizable sourceSynchronizable)
                {
                    collectionSynchronizable.UpdateLocker(sourceSynchronizable.Locker);
                    sourceSynchronizable.UpdateLocker(collectionSynchronizable.Locker);
                }
            }
        }

        public bool OnRemoved(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, int originalIndex, int index, out bool isReset)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            if (Size > decorator.BatchThreshold)
            {
                isReset = true;
                Reset(decoratorManager, decorator);
            }
            else
            {
                isReset = false;
                foreach (var item in GetItems())
                    decoratorManager.OnRemoved(decorator.Owner, decorator, item, index);
            }

            Indexes.Remove(originalIndex);
            if (Indexes.Count == 0)
            {
                Detach();
                return true;
            }

            return false;
        }

        public void OnMoved(FlattenCollectionDecorator decorator, ICollectionDecoratorManagerComponent decoratorManager, int oldIndex, int newIndex)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            if (Size > decorator.BatchThreshold)
            {
                Reset(decoratorManager, decorator);
                return;
            }

            var removeIndex = 0;
            var addIndex = 0;
            if (oldIndex < newIndex)
                addIndex += Size - 1;
            foreach (var item in GetItems())
            {
                decoratorManager.OnMoved(decorator.Owner, decorator, item, oldIndex + removeIndex, newIndex + addIndex);
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
            Indexes.Clear();
            if (Collection is IReadOnlyObservableCollection owner)
                owner.Components.Remove(this);

            var tokens = _tokens;
            if (!tokens.IsEmpty)
            {
                _tokens = default;
                for (var i = 0; i < tokens.Count; i++)
                    tokens.Items[i].Dispose();
            }
        }

        public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            for (var i = 0; i < Indexes.Count; i++)
                decoratorManager.OnChanged(decorator.Owner, decorator, item, decorator.GetIndex(Indexes.Items[i]) + index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            ++Size;
            for (var i = 0; i < Indexes.Count; i++)
                decoratorManager.OnAdded(decorator.Owner, decorator, item, decorator.GetIndex(Indexes.Items[i]) + index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            for (var i = 0; i < Indexes.Count; i++)
                decoratorManager.OnReplaced(decorator.Owner, decorator, oldItem, newItem, decorator.GetIndex(Indexes.Items[i]) + index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            for (var i = 0; i < Indexes.Count; i++)
            {
                var originalIndex = decorator.GetIndex(Indexes.Items[i]);
                decoratorManager.OnMoved(decorator.Owner, decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
            }
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (!TryGetDecoratorManager(out var decoratorManager, out var decorator))
                return;

            --Size;
            for (var i = 0; i < Indexes.Count; i++)
                decoratorManager.OnRemoved(decorator.Owner, decorator, item, decorator.GetIndex(Indexes.Items[i]) + index);
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
            using var _ = MugenExtensions.TryLock(Collection);
            if (TryGetDecoratorManager(out var decoratorManager, out var decorator))
                AddToken(decoratorManager.BatchUpdate(decorator.Owner, decorator));
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            RemoveToken();
        }

        public void OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
        {
            if (TryGetDecoratorManager(out _, out var decorator) && decorator.OwnerOptional is ISynchronizable synchronizable)
                synchronizable.UpdateLocker(locker);
        }

        protected abstract IEnumerable<object?> GetItems();

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

        private void AddToken(ActionToken actionToken)
        {
            if (_detached)
                actionToken.Dispose();
            else
            {
                if (_tokens.IsEmpty)
                    _tokens = new ListInternal<ActionToken>(2);
                _tokens.Add(actionToken);
            }
        }

        private void RemoveToken()
        {
            if (_tokens.Count == 0)
                return;

            var item = _tokens.Items[_tokens.Count - 1];
            _tokens.RemoveAt(_tokens.Count - 1);
            item.Dispose();
        }
    }
}