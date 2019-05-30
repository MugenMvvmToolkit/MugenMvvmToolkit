using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IHasListeners<IObservableCollectionListener<T>>, IList<T>
    {
        IComponentCollection<IObservableCollectionDecorator<T>> Decorators { get; }

        IComponentCollection<IObservableCollectionListener<T>> DecoratorListeners { get; }

        IEnumerable<T> DecorateItems();

        IDisposable BeginBatchUpdate(BatchUpdateCollectionMode mode = BatchUpdateCollectionMode.Both);

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<T> items);

        void RaiseItemChanged(T item, object? args);
    }
}