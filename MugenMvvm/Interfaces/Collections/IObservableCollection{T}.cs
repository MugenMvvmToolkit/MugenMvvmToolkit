using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>, IList<T>
    {
        new int Count { get; }

        new T this[int index] { get; set; }

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<T>? items);
    }
}