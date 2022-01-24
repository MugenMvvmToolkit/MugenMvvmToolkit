using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableList<T> : IObservableCollection<T>, IReadOnlyList<T>, IList<T>
    {
        new T this[int index] { get; set; }

        void Move(int oldIndex, int newIndex);
    }
}