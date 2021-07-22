using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangedListener<in T> : IComponent<IReadOnlyObservableCollection>
    {
        void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index);

        void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index);

        void OnReset(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items);
    }
}