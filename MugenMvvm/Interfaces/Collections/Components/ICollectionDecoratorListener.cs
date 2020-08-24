using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorListener : IComponent<ICollection>
    {
        void OnItemChanged(ICollection collection, object? item, int index, object? args);

        void OnAdded(ICollection collection, object? item, int index);

        void OnReplaced(ICollection collection, object? oldItem, object? newItem, int index);

        void OnMoved(ICollection collection, object? item, int oldIndex, int newIndex);

        void OnRemoved(ICollection collection, object? item, int index);

        void OnReset(ICollection collection, IEnumerable<object?>? items);
    }
}