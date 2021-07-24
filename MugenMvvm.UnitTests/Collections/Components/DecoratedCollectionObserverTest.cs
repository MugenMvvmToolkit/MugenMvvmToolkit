using MugenMvvm.Collections.Components;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class DecoratedCollectionObserverTest : CollectionObserverBaseTest
    {
        public DecoratedCollectionObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        protected override CollectionObserverBase GetObserver() => new DecoratedCollectionObserver();
    }
}