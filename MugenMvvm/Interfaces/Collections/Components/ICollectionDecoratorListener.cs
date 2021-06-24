using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorListener : IComponent<IReadOnlyObservableCollection>
    {
        void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args);

        void OnAdded(IReadOnlyObservableCollection collection, object? item, int index);

        void OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index);

        void OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex);

        void OnRemoved(IReadOnlyObservableCollection collection, object? item, int index);

        void OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items);
    }
}