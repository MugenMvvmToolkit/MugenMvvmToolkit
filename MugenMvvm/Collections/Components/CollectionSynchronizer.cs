using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    internal sealed class CollectionSynchronizer<T> : CollectionSynchronizerBase<T>, ICollectionChangedListener<T>
    {
        public CollectionSynchronizer(IList<T> target) : base(target, BatchUpdateType.Source)
        {
        }

        void ICollectionChangedListener<T>.OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAdded(collection, item, index);

        void ICollectionChangedListener<T>.OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) =>
            OnReplaced(collection, oldItem, newItem, index);

        void ICollectionChangedListener<T>.OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) =>
            OnMoved(collection, item, oldIndex, newIndex);

        void ICollectionChangedListener<T>.OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) => OnRemoved(collection, item, index);

        void ICollectionChangedListener<T>.OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items) => OnReset(collection, items);
    }
}