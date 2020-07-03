using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangingListener : IComponent<IObservableCollection>
    {
        void OnAdding<T>(IObservableCollection<T> collection, T item, int index);

        void OnReplacing<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoving<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoving<T>(IObservableCollection<T> collection, T item, int index);

        void OnResetting<T>(IObservableCollection<T> collection, IEnumerable<T> items);

        void OnClearing<T>(IObservableCollection<T> collection);
    }
}