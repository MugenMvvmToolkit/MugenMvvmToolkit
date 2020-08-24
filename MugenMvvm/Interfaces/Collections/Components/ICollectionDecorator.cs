using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecorator : IComponent<IObservableCollection>
    {
        IEnumerable<object?> DecorateItems(IObservableCollection collection, IEnumerable<object?> items);

        bool OnItemChanged(IObservableCollection collection, ref object? item, ref int index, ref object? args);

        bool OnAdded(IObservableCollection collection, ref object? item, ref int index);

        bool OnReplaced(IObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index);

        bool OnMoved(IObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex);

        bool OnRemoved(IObservableCollection collection, ref object? item, ref int index);

        bool OnReset(IObservableCollection collection, ref IEnumerable<object?>? items);
    }
}