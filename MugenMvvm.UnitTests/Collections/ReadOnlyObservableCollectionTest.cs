using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    [Collection(SharedContext)]
    public class ReadOnlyObservableCollectionTest : ReadOnlyObservableCollectionTestBase
    {
        protected override IReadOnlyObservableCollection<T> GetCollection<T>(IReadOnlyObservableCollection<T> source, bool disposeSource) =>
            new ReadOnlyObservableCollection<T>(source, 0, disposeSource, ComponentCollectionManager);

        public override void ShouldSynchronizeBatchUpdates(int batchUpdateType)
        {
            if (batchUpdateType == 2)
                return;
            base.ShouldSynchronizeBatchUpdates(batchUpdateType);
        }
    }
}