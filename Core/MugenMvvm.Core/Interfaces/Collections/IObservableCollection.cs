using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IComponentOwner<IObservableCollection<T>>, IList<T>
    {
        ActionToken BeginBatchUpdate();

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<T> items);

        void RaiseItemChanged(T item, object? args);
    }
}