using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyObservableCollection, IReadOnlyCollection<T>
    {
        new int Count { get; }
    }
}