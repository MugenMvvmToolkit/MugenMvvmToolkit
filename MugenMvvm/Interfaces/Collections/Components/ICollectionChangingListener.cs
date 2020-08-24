using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangingListener<in T> : IComponent<ICollection>
    {
        void OnAdding(IReadOnlyCollection<T> collection, T item, int index);

        void OnReplacing(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoving(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoving(IReadOnlyCollection<T> collection, T item, int index);

        void OnResetting(IReadOnlyCollection<T> collection, IEnumerable<T>? items);
    }
}