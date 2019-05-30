using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionConditionListener<T> : IObservableCollectionListener<T>
    {
        bool CanAdd(IObservableCollection<T> collection, T item, int index);

        bool CanReplace(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        bool CanMove(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        bool CanRemove(IObservableCollection<T> collection, T item, int index);

        bool CanReset(IObservableCollection<T> collection, IEnumerable<T> items);

        bool CanClear(IObservableCollection<T> collection);
    }
}