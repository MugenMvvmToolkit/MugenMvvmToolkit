using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Components
{
    public class ComponentCollectionProviderTest : ComponentOwnerTestBase<IComponentCollectionProvider>
    {
        #region Methods

        [Fact]
        public void GetComponentCollectionShouldUseComponentCollectionAsFallback()
        {
            var collectionProvider = GetComponentOwner();
            var componentCollection = collectionProvider.GetComponentCollection(this);
            componentCollection.ShouldBeType<ComponentCollection>();
        }

        [Fact]
        public void GetComponentCollectionShouldBeHandledByComponents()
        {
            var executed = 0;
            var result = new ComponentCollection(this);
            var collectionProvider = GetComponentOwner();
            var component = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (o, context) =>
                {
                    ++executed;
                    o.ShouldEqual(this);
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            collectionProvider.AddComponent(component);

            collectionProvider.GetComponentCollection(this, DefaultMetadata).ShouldEqual(result);
            executed.ShouldEqual(1);
        }

        [Fact]
        public void GetComponentCollectionShouldNotifyListeners()
        {
            var executed = 0;
            var result = new ComponentCollection(this);
            var collectionProvider = GetComponentOwner();
            var component = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (o, context) => result
            };
            collectionProvider.AddComponent(component);

            var listener = new TestComponentCollectionProviderListener();
            listener.OnComponentCollectionCreated = (provider, collection, arg3) =>
            {
                executed++;
                provider.ShouldEqual(collectionProvider);
                collection.ShouldEqual(result);
                arg3.ShouldEqual(DefaultMetadata);
            };
            collectionProvider.AddComponent(listener);

            collectionProvider.GetComponentCollection(this, DefaultMetadata).ShouldEqual(result);
            executed.ShouldEqual(1);
        }

        public override void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
        }

        protected override IComponentCollectionProvider GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ComponentCollectionProvider();
        }

        #endregion
    }
}