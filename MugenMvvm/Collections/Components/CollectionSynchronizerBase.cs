using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    internal abstract class CollectionSynchronizerBase<T> : ICollectionBatchUpdateListener, IDetachableComponent
    {
        private readonly IWeakReference _targetRef;
        private readonly BatchUpdateType _batchUpdateType;
        private ListInternal<ActionToken> _tokens;

        protected CollectionSynchronizerBase(IList<T> target, BatchUpdateType batchUpdateType)
        {
            Should.NotBeNull(target, nameof(target));
            _targetRef = target.ToWeakReference();
            _batchUpdateType = batchUpdateType;
            _tokens = new ListInternal<ActionToken>(0);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, T item, int index) => GetTarget(collection)?.Insert(index, item);

        public void OnReplaced(IReadOnlyObservableCollection collection, T oldItem, T newItem, int index)
        {
            var target = GetTarget(collection);
            if (target != null)
                target[index] = newItem;
        }

        public void OnMoved(IReadOnlyObservableCollection collection, T item, int oldIndex, int newIndex)
        {
            var target = GetTarget(collection);
            if (target == null)
                return;

            if (target is IObservableCollection<T> observableCollection)
                observableCollection.Move(oldIndex, newIndex);
            else
            {
                target.RemoveAt(oldIndex);
                target.Insert(newIndex, item);
            }
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, T item, int index) => GetTarget(collection)?.RemoveAt(index);

        public void OnReset(IReadOnlyObservableCollection collection, IEnumerable<T>? items) => GetTarget(collection)?.Reset(items);

        public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != _batchUpdateType || GetTarget(collection) is not IObservableCollection<T> observableCollection)
                return;

            lock (this)
            {
                _tokens.Add(observableCollection.BatchUpdate());
            }
        }

        public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            if (batchUpdateType != _batchUpdateType || GetTarget(collection) is not IObservableCollection<T>)
                return;

            lock (this)
            {
                if (_tokens.Count != 0)
                {
                    var item = _tokens.Items[_tokens.Count - 1];
                    _tokens.RemoveAt(_tokens.Count - 1);
                    item.Dispose();
                }
            }
        }

        protected IList<T>? GetTarget(IReadOnlyObservableCollection collection)
        {
            var target = (IList<T>?) _targetRef.Target;
            if (target == null)
            {
                collection.Components.Remove(this);
                return null;
            }

            return target;
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IReadOnlyObservableCollection)
            {
                lock (this)
                {
                    for (var i = 0; i < _tokens.Count; i++)
                        _tokens.Items[i].Dispose();
                    _tokens.Clear();
                }
            }
        }
    }
}