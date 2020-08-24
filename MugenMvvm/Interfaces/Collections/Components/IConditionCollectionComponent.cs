using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IConditionCollectionComponent<in T> : IComponent<ICollection>
    {
        bool CanAdd(IReadOnlyCollection<T> collection, T item, int index);

        bool CanReplace(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index);

        bool CanMove(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex);

        bool CanRemove(IReadOnlyCollection<T> collection, T item, int index);

        bool CanReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items);
    }
}