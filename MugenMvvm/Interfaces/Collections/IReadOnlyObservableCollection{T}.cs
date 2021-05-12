using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyObservableCollection, IReadOnlyList<T>
    {
        new int Count { get; }

        new T this[int index] { get; }
    }
}