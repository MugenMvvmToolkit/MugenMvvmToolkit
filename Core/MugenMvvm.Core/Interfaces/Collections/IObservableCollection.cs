using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IHasListeners<IObservableCollectionChangedListener<T>>, IList<T>
    {
        IComponentCollection<IObservableCollectionDecorator<T>> Decorators { get; }

        IComponentCollection<IObservableCollectionChangedListener<T>> DecoratorListeners { get; }

        IEnumerable<T> DecorateItems();

        IDisposable BeginBatchUpdate();

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<T> items);
    }
}