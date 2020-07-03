using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
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
            IComponentCollectionManager? manager = null;
            ComponentCollection? collection = null;
            var testComponentCollectionProviderComponent = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, o, context) =>
                {
                    c.ShouldEqual(globalValue ? MugenService.ComponentCollectionManager : manager);
                    componentOwner.ShouldEqual(o);
                    collection = new ComponentCollection(componentOwner!);
                    return collection;
                }
            };
            using var subscriber = globalValue ? TestComponentSubscriber.Subscribe(testComponentCollectionProviderComponent) : default;
            if (globalValue)
                componentOwner = GetComponentOwner();
            else
            {
                manager = new ComponentCollectionManager();
                manager.AddComponent(testComponentCollectionProviderComponent);
                componentOwner = GetComponentOwner(manager);
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

        protected abstract T GetComponentOwner(IComponentCollectionManager? collectionProvider = null);

        #endregion
    }
}