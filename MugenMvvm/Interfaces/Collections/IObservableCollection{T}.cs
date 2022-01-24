using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, ICollection<T>
    {
        new int Count { get; }

        bool Reset(IEnumerable<T>? items);
    }
}