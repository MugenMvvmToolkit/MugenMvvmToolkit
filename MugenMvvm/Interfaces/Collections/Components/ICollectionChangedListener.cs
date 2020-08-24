using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangedListener<in T> : IComponent<ICollection>
    {
        void OnItemChanged(IReadOnlyCollection<T> collection, T item, int index, object? args);

        void OnAdded(IReadOnlyCollection<T> collection, T item, int index);

        void OnReplaced(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoved(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoved(IReadOnlyCollection<T> collection, T item, int index);

        void OnReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items);
    }
}