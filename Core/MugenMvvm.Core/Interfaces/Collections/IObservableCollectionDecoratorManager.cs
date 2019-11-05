using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionDecoratorManager<T>
    {
        ActionToken Lock();

        IEnumerable<T> DecorateItems(IDecoratorObservableCollectionComponent<T> decorator);

        void OnItemChanged(IDecoratorObservableCollectionComponent<T> decorator, T item, int index, object? args);

        void OnAdded(IDecoratorObservableCollectionComponent<T> decorator, T item, int index);

        void OnReplaced(IDecoratorObservableCollectionComponent<T> decorator, T oldItem, T newItem, int index);

        void OnMoved(IDecoratorObservableCollectionComponent<T> decorator, T item, int oldIndex, int newIndex);

        void OnRemoved(IDecoratorObservableCollectionComponent<T> decorator, T item, int index);

        void OnReset(IDecoratorObservableCollectionComponent<T> decorator, IEnumerable<T> items);

        void OnCleared(IDecoratorObservableCollectionComponent<T> decorator);
    }
}