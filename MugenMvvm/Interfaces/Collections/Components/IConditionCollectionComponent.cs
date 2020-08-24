using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IConditionCollectionComponent : IComponent<IObservableCollection>
    {
        bool CanAdd(IObservableCollection collection, object? item, int index);

        bool CanReplace(IObservableCollection collection, object? oldItem, object? newItem, int index);

        bool CanMove(IObservableCollection collection, object? item, int oldIndex, int newIndex);

        bool CanRemove(IObservableCollection collection, object? item, int index);

        bool CanReset(IObservableCollection collection, IEnumerable<object?>? items);
    }
}