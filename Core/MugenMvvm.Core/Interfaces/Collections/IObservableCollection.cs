using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IHasListeners<IObservableCollectionChangedListener>, IList<T>
    {
        void Move(int oldIndex, int newIndex);

        IDisposable BeginBatchUpdate();
    }
}