using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection : IReadOnlyObservableCollection, IList
    {
        new int Count { get; }

        new object? this[int index] { get; set; }

        ActionToken BeginBatchUpdate();

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<object> items);

        void RaiseItemChanged(object item, object? args);
    }
}