using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionChangedListener<T> : IListener
    {
        void OnBeginBatchUpdate(IObservableCollection<T> collection);

        void OnEndBatchUpdate(IObservableCollection<T> collection);

        void OnAdded(IObservableCollection<T> collection, T item, int index);

        void OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoved(IObservableCollection<T> collection, T item, int index);

        void OnReset(IObservableCollection<T> collection);

        void OnCleared(IObservableCollection<T> collection);
    }
}