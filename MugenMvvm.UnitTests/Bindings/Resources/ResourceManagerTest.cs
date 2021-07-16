using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Bindings.Resources;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Resources
{
    public class ResourceManagerTest : ComponentOwnerTestBase<ResourceManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetResourceValueShouldBeHandledByComponents(int componentCount)
        {
            var name = "name";
            var request = this;
            var result = new ResourceResolverResult(this);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                ResourceManager.AddComponent(new TestResourceResolverComponent
                {
                    Priority = -i,
                    TryGetResource = (rm, s, o, arg4) =>
                    {
                        ++invokeCount;
                        rm.ShouldEqual(ResourceManager);
                        s.ShouldEqual(name);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(Metadata);
                        if (isLast)
                            return result;
                        return default;
                    }
                });
            }

            ResourceManager.TryGetResource(name, request, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTypeShouldBeHandledByComponents(int componentCount)
        {
            var name = "name";
            var request = this;
            var result = typeof(string);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                ResourceManager.AddComponent(new TestTypeResolverComponent
                {
                    Priority = -i,
                    TryGetType = (rm, s, o, arg4) =>
                    {
                        ++invokeCount;
                        rm.ShouldEqual(ResourceManager);
                        s.ShouldEqual(name);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(Metadata);
                        if (isLast)
                            return result;
                        return null;
                    }
                });
            }

            ResourceManager.TryGetType(name, request, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override IResourceManager GetResourceManager() => GetComponentOwner(ComponentCollectionManager);

        protected override ResourceManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}