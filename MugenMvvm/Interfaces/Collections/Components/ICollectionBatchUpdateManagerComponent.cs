using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionBatchUpdateManagerComponent : IComponent<IReadOnlyObservableCollection>
    {
        bool IsInBatch(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType);

        void BeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType);

        void EndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType);
    }
}