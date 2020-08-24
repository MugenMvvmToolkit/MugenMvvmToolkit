using System.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionBatchUpdateListener : IComponent<ICollection>
    {
        void OnBeginBatchUpdate(ICollection collection);

        void OnEndBatchUpdate(ICollection collection);
    }
}