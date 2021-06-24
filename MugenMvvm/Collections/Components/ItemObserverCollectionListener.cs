using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    public class ItemObserverCollectionListener<T> : ItemObserverCollectionListenerBase<T>, ICollectionChangedListener<T> where T : class?
    {
        public ItemObserverCollectionListener() : base(null)
        {
        }

        public ItemObserverCollectionListener(IEqualityComparer<T>? comparer) : base(comparer)
        {
        }

        void ICollectionChangedListener<T>.OnChanged(IReadOnlyObservableCollection<T> collection, T item, int index, object? args)
        {
        }

        void ICollectionChangedListener<T>.OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAdded(item);

        void ICollectionChangedListener<T>.OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) => OnReplaced(oldItem, newItem);

        void ICollectionChangedListener<T>.OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) => OnMoved(item);

        void ICollectionChangedListener<T>.OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) => OnRemoved(item);

        void ICollectionChangedListener<T>.OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items) => OnReset(null, items);
    }
}