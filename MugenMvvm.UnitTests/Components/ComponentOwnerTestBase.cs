using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Components
{
    [Collection(SharedContext)]
    public abstract class ComponentOwnerTestBase<T> : UnitTestBase where T : class, IComponentOwner
    {
        protected ComponentOwnerTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(ComponentCollectionManager));
        }

        [Fact]
        public virtual void ComponentOwnerShouldReturnCorrectHasComponentsValue()
        {
            var componentOwner = GetComponentOwner();
            componentOwner.HasComponents.ShouldBeFalse();

            componentOwner.Components.TryAdd(this);
            componentOwner.HasComponents.ShouldBeTrue();

            componentOwner.Components.Remove(this);
            componentOwner.HasComponents.ShouldBeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void ComponentOwnerShouldUseCollectionFactory(bool globalValue)
        {
            T? componentOwner = null;
            ComponentCollection? collection = null;
            var testComponentCollectionProviderComponent = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, o, context) =>
                {
                    c.ShouldEqual(ComponentCollectionManager);
                    componentOwner.ShouldEqual(o);
                    collection = new ComponentCollection(componentOwner!, c);
                    return collection;
                }
            };
            ComponentCollectionManager.AddComponent(testComponentCollectionProviderComponent);
            componentOwner = globalValue ? GetComponentOwner() : GetComponentOwner(ComponentCollectionManager);
            componentOwner.Components.ShouldEqual(collection);
        }

        protected abstract T GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null);
    }
}