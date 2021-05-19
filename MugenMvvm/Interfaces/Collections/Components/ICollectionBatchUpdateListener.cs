using System.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionBatchUpdateListener : IComponent<ICollection>
    {
        void OnBeginBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType);

        void OnEndBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType);
    }
}