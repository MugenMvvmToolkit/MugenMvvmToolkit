using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IConditionCollectionComponent<T> : IComponent<IObservableCollection>
    {
        bool CanAdd(IObservableCollection<T> collection, T item, int index);

        bool CanReplace(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        bool CanMove(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        bool CanRemove(IObservableCollection<T> collection, T item, int index);

        bool CanReset(IObservableCollection<T> collection, IEnumerable<T>? items);
    }
}