using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    internal abstract class CollectionSynchronizerBase<T> : ICollectionBatchUpdateListener, IDetachableComponent
    {
        protected readonly IList<T> Target;
        private readonly BatchUpdateType _batchUpdateType;
        private readonly ListInternal<ActionToken> _tokens;

        protected CollectionSynchronizerBase(IList<T> target, BatchUpdateType batchUpdateType)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            _batchUpdateType = batchUpdateType;
            _tokens = new ListInternal<ActionToken>(0);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, T item, int index) => Target.Insert(index, item);

        public void OnReplaced(IReadOnlyObservableCollection collection, T oldItem, T newItem, int index) => Target[index] = newItem;

        public void OnMoved(IReadOnlyObservableCollection collection, T item, int oldIndex, int newIndex)
        {
            if (Target is IObservableCollection<T> observableCollection)
                observableCollection.Move(oldIndex, newIndex);
            else
            {
                Target.RemoveAt(oldIndex);
                Target.Insert(newIndex, item);
            }
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, T item, int index) => Target.RemoveAt(index);

        public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<T>? items) => Target.Reset(items);

        public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != _batchUpdateType || Target is not IObservableCollection<T> observableCollection)
                return;

            lock (_tokens)
            {
                _tokens.Add(observableCollection.BatchUpdate());
            }
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != _batchUpdateType || Target is not IObservableCollection<T>)
                return;

            lock (_tokens)
            {
                if (_tokens.Count != 0)
                {
                    var item = _tokens.Items[_tokens.Count - 1];
                    _tokens.RemoveAt(_tokens.Count - 1);
                    item.Dispose();
                }
            }
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IReadOnlyObservableCollection)
            {
                lock (_tokens)
                {
                    for (var i = 0; i < _tokens.Count; i++)
                        _tokens.Items[i].Dispose();
                    _tokens.Clear();
                }
            }
        }
    }
}