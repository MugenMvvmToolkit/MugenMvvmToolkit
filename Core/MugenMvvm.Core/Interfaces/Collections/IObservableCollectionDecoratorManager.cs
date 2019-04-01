using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionDecoratorManager<T>
    {
        IObservableCollection<T> Collection { get; }

        IDisposable Lock();

        IEnumerable<T> DecorateItems(IObservableCollectionDecorator<T> decorator);

        void OnItemChanged(IObservableCollectionDecorator<T> decorator, T item, int index, object? args);

        void OnAdded(IObservableCollectionDecorator<T> decorator, T item, int index);

        void OnReplaced(IObservableCollectionDecorator<T> decorator, T oldItem, T newItem, int index);

        void OnMoved(IObservableCollectionDecorator<T> decorator, T item, int oldIndex, int newIndex);

        void OnRemoved(IObservableCollectionDecorator<T> decorator, T item, int index);

        void OnReset(IObservableCollectionDecorator<T> decorator, IEnumerable<T> items);

        void OnCleared(IObservableCollectionDecorator<T> decorator);
    }
}