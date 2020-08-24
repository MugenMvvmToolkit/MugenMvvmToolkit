using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangingListener : IComponent<IObservableCollection>
    {
        void OnAdding(IObservableCollection collection, object? item, int index);

        void OnReplacing(IObservableCollection collection, object? oldItem, object? newItem, int index);

        void OnMoving(IObservableCollection collection, object? item, int oldIndex, int newIndex);

        void OnRemoving(IObservableCollection collection, object? item, int index);

        void OnResetting(IObservableCollection collection, IEnumerable<object?>? items);
    }
}