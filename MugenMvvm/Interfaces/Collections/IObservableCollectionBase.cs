using System.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionBase : IEnumerable, IComponentOwner<IObservableCollection>
    {
        ActionToken BeginBatchUpdate();

        void Move(int oldIndex, int newIndex);
    }
}