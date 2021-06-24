using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionBatchUpdateListener : IComponent<IReadOnlyObservableCollection>
    {
        void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType);

        void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType);
    }
}