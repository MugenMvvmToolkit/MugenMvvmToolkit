using System.Collections;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection : IReadOnlyObservableCollection, IList
    {
        new int Count { get; }

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable? items);
    }
}