using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Components
{
    public class ComponentCollectionManagerTest : ComponentOwnerTestBase<IComponentCollectionManager>
    {
        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
        }

        [Fact]
        public void GetComponentCollectionShouldBeHandledByComponents()
        {
            var executed = 0;
            var result = new ComponentCollection(this, ComponentCollectionManager);

            ComponentCollectionManager.AddComponent(new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, o, context) =>
                {
                    ++executed;
                    c.ShouldEqual(ComponentCollectionManager);
                    o.ShouldEqual(this);
                    context.ShouldEqual(Metadata);
                    return result;
                }
            });

            ComponentCollectionManager.GetComponentCollection(this, Metadata).ShouldEqual(result);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void GetComponentCollectionShouldNotifyListeners()
        {
            var executed = 0;
            var result = new ComponentCollection(this, ComponentCollectionManager);
            ComponentCollectionManager.AddComponent(new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, _, _) =>
                {
                    c.ShouldEqual(ComponentCollectionManager);
                    return result;
                }
            });
            ComponentCollectionManager.AddComponent(new TestComponentCollectionManagerListener
            {
                OnComponentCollectionCreated = (provider, collection, arg3) =>
                {
                    executed++;
                    provider.ShouldEqual(ComponentCollectionManager);
                    collection.ShouldEqual(result);
                    arg3.ShouldEqual(Metadata);
                }
            });
            ComponentCollectionManager.GetComponentCollection(this, Metadata).ShouldEqual(result);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void GetComponentCollectionShouldUseComponentCollectionAsFallback() => ComponentCollectionManager.GetComponentCollection(this).ShouldBeType<ComponentCollection>();

        protected override IComponentCollectionManager GetComponentCollectionManager() => GetComponentOwner();

        protected override IComponentCollectionManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new ComponentCollectionManager();
    }
}