﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IObservableCollectionChangedDecoratorListener<T> : IComponent<IObservableCollection<T>>
    {
        void OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args);

        void OnAdded(IObservableCollection<T> collection, T item, int index);

        void OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        void OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        void OnRemoved(IObservableCollection<T> collection, T item, int index);

        void OnReset(IObservableCollection<T> collection, IEnumerable<T> items);

        void OnCleared(IObservableCollection<T> collection);
    }
}