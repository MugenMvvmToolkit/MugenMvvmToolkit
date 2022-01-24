using System.Collections;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection : IReadOnlyObservableCollection, ICollection
    {
        new int Count { get; }

        bool Reset(IEnumerable? items);
    }
}