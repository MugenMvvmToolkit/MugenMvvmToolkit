using System.Collections;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionChangedListener
    {
        void OnBeginBatchUpdate(IEnumerable collection);

        void OnEndBatchUpdate(IEnumerable collection);

        void OnAdded(IEnumerable collection, object item, int index);

        void OnReplaced(IEnumerable collection, object oldItem, object newItem, int index);

        void OnMoved(IEnumerable collection, object item, int oldIndex, int newIndex);

        void OnRemoved(IEnumerable collection, object item, int index);

        void OnCleared(IEnumerable collection);
    }
}