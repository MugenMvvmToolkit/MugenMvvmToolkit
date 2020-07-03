using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorListener : IComponent<IObservableCollection>
    {
        void OnItemChanged(IObservableCollection collection, object? item, int index, object? args);

        void OnAdded(IObservableCollection collection, object? item, int index);

        void OnReplaced(IObservableCollection collection, object? oldItem, object? newItem, int index);

        void OnMoved(IObservableCollection collection, object? item, int oldIndex, int newIndex);

        void OnRemoved(IObservableCollection collection, object? item, int index);

        void OnReset(IObservableCollection collection, IEnumerable<object?> items);

        void OnCleared(IObservableCollection collection);
    }
}