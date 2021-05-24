using MugenMvvm.Collections.Components;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ItemObserverCollectionDecoratorListenerTest : ItemObserverCollectionListenerBaseTest
    {
        public ItemObserverCollectionDecoratorListenerTest(ITestOutputHelper? outputHelper = null) : base(new ItemObserverCollectionDecoratorListener(), outputHelper)
        {
        }
    }
}