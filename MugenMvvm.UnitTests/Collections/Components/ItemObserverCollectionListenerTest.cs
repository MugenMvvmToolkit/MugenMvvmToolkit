using MugenMvvm.Collections.Components;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class ItemObserverCollectionListenerTest : ItemObserverCollectionListenerBaseTest
    {
        public ItemObserverCollectionListenerTest(ITestOutputHelper? outputHelper = null) : base(new ItemObserverCollectionListener<object?>(), outputHelper)
        {
        }
    }
}