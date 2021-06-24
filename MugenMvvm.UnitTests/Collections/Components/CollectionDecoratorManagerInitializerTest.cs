using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Collections;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class CollectionDecoratorManagerInitializerTest : UnitTestBase
    {
        public CollectionDecoratorManagerInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ComponentCollectionManager.AddComponent(new CollectionDecoratorManagerInitializer());
        }

        [Fact]
        public void ShouldAddDecoratorManager()
        {
            var collection = new SynchronizedObservableCollection<int>(ComponentCollectionManager);
            collection.Components.GetComponentOptional<CollectionDecoratorManagerInitializer>().ShouldNotBeNull();
            collection.GetComponentOptional<CollectionDecoratorManager<int>>().ShouldBeNull();

            collection.AddComponent(new TestCollectionDecoratorListener<object>());
            collection.Components.GetComponentOptional<CollectionDecoratorManagerInitializer>().ShouldBeNull();
            collection.GetComponentOptional<CollectionDecoratorManager<int>>().ShouldNotBeNull();
        }

        protected override IComponentCollectionManager GetComponentCollectionManager() => new ComponentCollectionManager();
    }
}