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
    public sealed class CollectionSynchronizer<T> : ICollectionChangedListener<T>, ICollectionBatchUpdateListener, IDetachableComponent
    {
        private readonly IList<T> _target;
        private readonly ListInternal<ActionToken> _tokens;

        public CollectionSynchronizer(IList<T> target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
            _tokens = new ListInternal<ActionToken>(0);
        }

        public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Source && _target is IObservableCollection<T> observableCollection)
            {
                lock (_tokens)
                {
                    _tokens.Add(observableCollection.BatchUpdate());
                }
            }
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType == BatchUpdateType.Source && _target is IObservableCollection<T>)
            {
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
        }

        public void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => _target.Insert(index, item);

        public void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) => _target[index] = newItem;

        public void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            if (_target is IObservableCollection<T> observableCollection)
                observableCollection.Move(oldIndex, newIndex);
            else
            {
                _target.RemoveAt(oldIndex);
                _target.Insert(newIndex, item);
            }
        }

        public void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) => _target.RemoveAt(index);

        public void OnReset(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items)
        {
            if (_target is IObservableCollection<T> observableCollection)
                observableCollection.Reset(items);
            else
            {
                _target.Clear();
                if (items != null)
                    _target.AddRange(items);
            }
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IReadOnlyObservableCollection<T>)
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