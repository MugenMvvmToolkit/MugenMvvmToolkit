using System.Collections;
using System.Collections.Generic;
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
    internal abstract class FlattenCollectionItemBase : ListInternal<int>, ILockerChangedListener<IReadOnlyObservableCollection>, IHasPriority
    {
        internal IEnumerable Collection = null!;
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

        public void FindAllIndexOf(object item, ref ItemOrListEditor<int> indexes)
        {
            if (Count == 0)
                return;

            int index = 0;
            using var _ = MugenExtensions.TryLock(Collection);
            foreach (var value in GetItems())
            {
                if (Equals(item, value))
                {
                    for (int i = 0; i < Count; i++)
                        indexes.Add(Decorator.GetIndex(Items[i]) + index);
                }

                ++index;
            }
        }

        public void OnAdded(object source, int originalIndex, int index, bool notify, bool isRecycled, out bool isReset)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            isReset = false;
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
                if (Collection is IReadOnlyObservableCollection owner)
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
            if (Collection is IReadOnlyObservableCollection owner)
                owner.Components.Remove(this);

            var tokens = _tokens;
            if (tokens != null)
            {
                _tokens = null;
                for (var i = 0; i < tokens.Count; i++)
                    tokens.Items[i].Dispose();
            }
        }

        public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            if (DecoratorManager == null)
                return;

            for (var i = 0; i < Count; i++)
                DecoratorManager.OnChanged(Decorator.Owner, Decorator, item, Decorator.GetIndex(Items[i]) + index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (DecoratorManager == null)
                return;

            ++Size;
            for (var i = 0; i < Count; i++)
                DecoratorManager.OnAdded(Decorator.Owner, Decorator, item, Decorator.GetIndex(Items[i]) + index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            if (DecoratorManager == null)
                return;

            for (var i = 0; i < Count; i++)
                DecoratorManager.OnReplaced(Decorator.Owner, Decorator, oldItem, newItem, Decorator.GetIndex(Items[i]) + index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            if (DecoratorManager == null)
                return;

            for (var i = 0; i < Count; i++)
            {
                var originalIndex = Decorator.GetIndex(Items[i]);
                DecoratorManager.OnMoved(Decorator.Owner, Decorator, item, originalIndex + oldIndex, originalIndex + newIndex);
            }
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (DecoratorManager == null)
                return;

            --Size;
            for (var i = 0; i < Count; i++)
                DecoratorManager.OnRemoved(Decorator.Owner, Decorator, item, Decorator.GetIndex(Items[i]) + index);
        }

        public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return;

            Size = items.CountEx();
            Reset();
        }

        public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            if (DecoratorManager != null)
                AddToken(DecoratorManager.BatchUpdate(Decorator.Owner, Decorator));
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            using var _ = MugenExtensions.TryLock(Collection);
            RemoveToken();
        }

        public void OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
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
    }
}