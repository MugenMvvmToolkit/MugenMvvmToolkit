using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangingListener<T> : IComponent<IObservableCollection>
    {
        void OnAdding(IObservableCollection<T> collection, T item, int index);

        void OnReplacing(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoving(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoving(IObservableCollection<T> collection, T item, int index);

        void OnResetting(IObservableCollection<T> collection, IEnumerable<T>? items);
    }
}