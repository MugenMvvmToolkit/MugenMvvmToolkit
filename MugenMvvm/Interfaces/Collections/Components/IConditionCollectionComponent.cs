using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IConditionCollectionComponent<in T> : IComponent<IReadOnlyObservableCollection>
    {
        bool CanAdd(IReadOnlyObservableCollection<T> collection, T item, int index);

        bool CanReplace(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index);

        bool CanMove(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        bool CanRemove(IReadOnlyObservableCollection<T> collection, T item, int index);

        bool CanReset(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items);
    }
}