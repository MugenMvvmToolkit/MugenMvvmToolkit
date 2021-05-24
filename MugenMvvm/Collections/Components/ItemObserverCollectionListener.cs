using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    public class ItemObserverCollectionListener<T> : ItemObserverCollectionListenerBase<T>, ICollectionChangingListener<T> where T : class?
    {
        public ItemObserverCollectionListener() : base(null)
        {
        }

        public ItemObserverCollectionListener(IEqualityComparer<T>? comparer) : base(comparer)
        {
        }

        void ICollectionChangingListener<T>.OnAdding(IReadOnlyCollection<T> collection, T item, int index) => OnAdded(item);

        void ICollectionChangingListener<T>.OnReplacing(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index) => OnReplaced(oldItem, newItem);

        void ICollectionChangingListener<T>.OnMoving(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex) => OnMoved(item);

        void ICollectionChangingListener<T>.OnRemoving(IReadOnlyCollection<T> collection, T item, int index) => OnRemoved(item);

        void ICollectionChangingListener<T>.OnResetting(IReadOnlyCollection<T> collection, IEnumerable<T>? items) => OnReset(collection, items);
    }
}