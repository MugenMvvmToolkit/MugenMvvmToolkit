using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IDecoratorManagerObservableCollectionComponent<T> : IComponent<IObservableCollection<T>>
    {
        IEnumerable<T> DecorateItems(IObservableCollectionDecorator<T>? decorator = null);

        void OnItemChanged(IObservableCollectionDecorator<T> decorator, T item, int index, object? args);

        void OnAdded(IObservableCollectionDecorator<T> decorator, T item, int index);

        void OnReplaced(IObservableCollectionDecorator<T> decorator, T oldItem, T newItem, int index);

        void OnMoved(IObservableCollectionDecorator<T> decorator, T item, int oldIndex, int newIndex);

        void OnRemoved(IObservableCollectionDecorator<T> decorator, T item, int index);

        void OnReset(IObservableCollectionDecorator<T> decorator, IEnumerable<T> items);

        void OnCleared(IObservableCollectionDecorator<T> decorator);
    }
}