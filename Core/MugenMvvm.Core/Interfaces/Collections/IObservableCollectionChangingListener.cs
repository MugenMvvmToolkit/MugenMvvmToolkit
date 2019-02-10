using System.Collections;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionChangingListener : IObservableCollectionChangedListener
    {
        bool OnAdding(IEnumerable collection, object item, int index);

        bool OnReplacing(IEnumerable collection, object oldItem, object newItem, int index);

        bool OnMoving(IEnumerable collection, object item, int oldIndex, int newIndex);

        bool OnRemoving(IEnumerable collection, object item, int index);

        bool OnClearing(IEnumerable collection);
    }
}