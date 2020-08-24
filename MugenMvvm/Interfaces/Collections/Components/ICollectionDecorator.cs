using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecorator : IComponent<ICollection>
    {
        IEnumerable<object?> DecorateItems(ICollection collection, IEnumerable<object?> items);

        bool OnItemChanged(ICollection collection, ref object? item, ref int index, ref object? args);

        bool OnAdded(ICollection collection, ref object? item, ref int index);

        bool OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index);

        bool OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex);

        bool OnRemoved(ICollection collection, ref object? item, ref int index);

        bool OnReset(ICollection collection, ref IEnumerable<object?>? items);
    }
}