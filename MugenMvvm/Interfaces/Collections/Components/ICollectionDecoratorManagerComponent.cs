using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorManagerComponent : IComponent<ICollection>
    {
        IEnumerable<object?> Decorate(ICollection collection, ICollectionDecorator? decorator = null);

        void OnChanged(ICollection collection, ICollectionDecorator? decorator, object? item, int index, object? args);

        void OnAdded(ICollection collection, ICollectionDecorator? decorator, object? item, int index);

        void OnReplaced(ICollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index);

        void OnMoved(ICollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex);

        void OnRemoved(ICollection collection, ICollectionDecorator? decorator, object? item, int index);

        void OnReset(ICollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items);
    }
}