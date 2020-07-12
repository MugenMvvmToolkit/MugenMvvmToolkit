using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangedListener : IComponent<IObservableCollection>//todo non generic and generic impls
    {
        void OnItemChanged<T>(IObservableCollection<T> collection, T item, int index, object? args);

        void OnAdded<T>(IObservableCollection<T> collection, T item, int index);

        void OnReplaced<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoved<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoved<T>(IObservableCollection<T> collection, T item, int index);

        void OnReset<T>(IObservableCollection<T> collection, IEnumerable<T> items);

        void OnCleared<T>(IObservableCollection<T> collection);
    }
}