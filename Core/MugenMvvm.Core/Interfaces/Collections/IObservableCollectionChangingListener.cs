using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionChangingListener<T> : IObservableCollectionChangedListener<T>
    {
        bool OnAdding(IObservableCollection<T> collection, T item, int index);

        bool OnReplacing(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        bool OnMoving(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        bool OnRemoving(IObservableCollection<T> collection, T item, int index);

        bool OnResetting(IObservableCollection<T> collection, IEnumerable<T> items);

        bool OnClearing(IObservableCollection<T> collection);
    }
}