using MugenMvvm.Collections.Components;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class CollectionObserverTest : CollectionObserverBaseTest
    {
        public CollectionObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        protected override CollectionObserverBase GetObserver() => new CollectionObserver();
    }
}