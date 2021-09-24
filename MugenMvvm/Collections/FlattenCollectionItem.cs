using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal sealed class FlattenCollectionItem<T> : FlattenCollectionItemBase, ICollectionChangedListener<T>, ICollectionBatchUpdateListener
    {
        private List<object?>? _sourceSnapshot;
        private bool _isInBatch;
        private bool _isDirty;

        [Preserve(Conditional = true)]
        public FlattenCollectionItem()
        {
        }

        public override void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Source || _isInBatch)
                return;

            _isInBatch = true;
            if (collection.Count > 0)
            {
                if (_sourceSnapshot == null)
                    _sourceSnapshot = new List<object?>(collection.AsEnumerable());
                else
                {
                    _sourceSnapshot.Clear();
                    _sourceSnapshot.AddRange(collection.AsEnumerable());
                }
            }
        }

        public override void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != BatchUpdateType.Source || !_isInBatch)
                return;

            var isValid = TryGetDecoratorManager(out var decoratorManager, out var decorator, out var owner);
            ActionToken token = default;
            try
            {
                if (isValid && !owner!.TryLock(0, out token)) //possible deadlock
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

                _isInBatch = false;
                _sourceSnapshot?.Clear();
                if (_isDirty)
                {
                    _isDirty = false;
                    if (isValid)
                    {
                        Size = collection.Count;
                        Reset(decoratorManager!, decorator!, owner!);
                    }
                }
            }
            finally
            {
                token.Dispose();
            }
        }

        public void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            if (_isInBatch)
                _isDirty = true;
            else
                OnAdded((IReadOnlyObservableCollection) collection, item, index);
        }

        public void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            if (_isInBatch)
                _isDirty = true;
            else
                OnReplaced((IReadOnlyObservableCollection) collection, oldItem, newItem, index);
        }

        public void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            if (_isInBatch)
                _isDirty = true;
            else
                OnMoved((IReadOnlyObservableCollection) collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            if (_isInBatch)
                _isDirty = true;
            else
                OnRemoved((IReadOnlyObservableCollection) collection, item, index);
        }

        public void OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items)
        {
            if (_isInBatch)
                _isDirty = true;
            else
                OnReset(collection, AsObjectEnumerable(items));
        }

        protected internal override IEnumerable<object?> GetItems()
        {
            if (_isInBatch)
                return _sourceSnapshot ?? Default.EmptyEnumerable<object?>();
            return Collection.AsEnumerable();
        }

        private static IEnumerable<object?>? AsObjectEnumerable(IEnumerable<T>? items) => items == null ? null : items as IEnumerable<object?> ?? items.Cast<object>();
    }
}