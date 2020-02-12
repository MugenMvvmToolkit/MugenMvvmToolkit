using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Components
{
    public abstract class ComponentOwnerTestBase<T> : UnitTestBase where T : class, IComponentOwner
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
            T? componentOwner = null;
            ComponentCollection? collection = null;
            var testComponentCollectionProviderComponent = new TestComponentCollectionProviderComponent();
            testComponentCollectionProviderComponent.TryGetComponentCollection = (o, context) =>
            {
                componentOwner.ShouldEqual(o);
                collection = new ComponentCollection(componentOwner!);
                return collection;
            };
            using var subscriber = globalValue ? TestComponentSubscriber.Subscribe(testComponentCollectionProviderComponent) : default;
            if (globalValue)
                componentOwner = GetComponentOwner();
            else
            {
                var provider = new ComponentCollectionProvider();
                provider.AddComponent(testComponentCollectionProviderComponent);
                componentOwner = GetComponentOwner(provider);
            }

            componentOwner.Components.ShouldEqual(collection);
        }

        [Fact]
        public virtual void ComponentOwnerShouldReturnCorrectHasComponentsValue()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.HasComponents.ShouldBeFalse();

            componentOwner.Components.Add(this);
            componentOwner.HasComponents.ShouldBeTrue();

            componentOwner.Components.Remove(this);
            componentOwner.HasComponents.ShouldBeFalse();
        }

        protected abstract T GetComponentOwner(IComponentCollectionProvider? collectionProvider = null);

        #endregion
    }
}