using System.Collections;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection : IObservableCollectionBase, IList
    {
        void Reset(IEnumerable<object> items);

        void RaiseItemChanged(object item, object? args);
    }
}