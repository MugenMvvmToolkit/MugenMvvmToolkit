using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangingListener<in T> : IComponent<IReadOnlyObservableCollection>
    {
        void OnAdding(IReadOnlyObservableCollection<T> collection, T item, int index);

        void OnReplacing(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoving(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoving(IReadOnlyObservableCollection<T> collection, T item, int index);

        void OnResetting(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items);
    }
}