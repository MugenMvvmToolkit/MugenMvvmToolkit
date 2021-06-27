using System;
using System.Collections;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection : IReadOnlyObservableCollection, IList, IDisposable
    {
        new int Count { get; }

        new object? this[int index] { get; set; }

        ActionToken BatchUpdate();

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable? items);

        void RaiseItemChanged(object item, object? args);
    }
}