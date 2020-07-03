using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IObservableCollectionBase, IList<T>
    {
        void Reset(IEnumerable<T> items);

        void RaiseItemChanged(T item, object? args);
    }
}