using System;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Components
{
    [Collection(SharedContext)]
    public abstract class ComponentOwnerTestBase<T> : UnitTestBase, IDisposable where T : class, IComponentOwner
    {
        protected ComponentOwnerTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            MugenService.Configuration.InitializeInstance<IComponentCollectionManager>(new ComponentCollectionManager());
        }

        public virtual void Dispose() => MugenService.Configuration.Clear<IComponentCollectionManager>();

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
            IComponentCollectionManager? manager = null;
            ComponentCollection? collection = null;
            var testComponentCollectionProviderComponent = new TestComponentCollectionProviderComponent
            {
                TryGetComponentCollection = (c, o, context) =>
                {
                    c.ShouldEqual(globalValue ? MugenService.ComponentCollectionManager : manager);
                    componentOwner.ShouldEqual(o);
                    collection = new ComponentCollection(componentOwner!, c);
                    return collection;
                }
            };
            using var t = globalValue ? MugenService.AddComponent(testComponentCollectionProviderComponent) : default;
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

        protected abstract T GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null);
    }
}