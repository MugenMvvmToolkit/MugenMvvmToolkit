using System.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionChangedListener : IListener
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