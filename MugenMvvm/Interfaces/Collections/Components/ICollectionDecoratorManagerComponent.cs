using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorManagerComponent : IComponent<IObservableCollection>
    {
        IEnumerable<object?> DecorateItems(IObservableCollection collection, ICollectionDecorator? decorator = null);

        void OnItemChanged(IObservableCollection collection, ICollectionDecorator decorator, object? item, int index, object? args);

        void OnAdded(IObservableCollection collection, ICollectionDecorator decorator, object? item, int index);

        void OnReplaced(IObservableCollection collection, ICollectionDecorator decorator, object? oldItem, object? newItem, int index);

        void OnMoved(IObservableCollection collection, ICollectionDecorator decorator, object? item, int oldIndex, int newIndex);

        void OnRemoved(IObservableCollection collection, ICollectionDecorator decorator, object? item, int index);

        void OnReset(IObservableCollection collection, ICollectionDecorator decorator, IEnumerable<object?>? items);
    }
}