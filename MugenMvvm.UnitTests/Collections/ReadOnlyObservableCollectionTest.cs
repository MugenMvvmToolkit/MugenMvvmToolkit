using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    [Collection(SharedContext)]
    public class ReadOnlyObservableCollectionTest : ReadOnlyObservableCollectionTestBase
    {
        protected override IReadOnlyObservableCollection<T> GetCollection<T>(IReadOnlyObservableCollection<T> source, bool disposeSource, bool isWeak = true) =>
            new ReadOnlyObservableCollection<T>(source, 0, disposeSource, isWeak, ComponentCollectionManager);

        public override void ShouldSynchronizeBatchUpdates(int batchUpdateType, bool supported) => base.ShouldSynchronizeBatchUpdates(batchUpdateType, batchUpdateType == 1);
    }
}