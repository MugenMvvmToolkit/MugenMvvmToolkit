using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorManagerComponent : IComponent<IReadOnlyObservableCollection>
    {
        ActionToken BatchUpdate(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator = null);

        IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator = null);

        void OnChanged(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index, object? args);

        void OnAdded(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index);

        void OnReplaced(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index);

        void OnMoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex);

        void OnRemoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index);

        void OnReset(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items);
    }
}