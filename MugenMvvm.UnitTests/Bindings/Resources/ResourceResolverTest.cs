using MugenMvvm.Bindings.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Bindings.Resources.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Resources
{
    public class ResourceResolverTest : ComponentOwnerTestBase<ResourceResolver>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetResourceValueShouldBeHandledByComponents(int componentCount)
        {
            var resolver = new ResourceResolver();
            var name = "name";
            var request = this;
            var result = new ResourceResolverResult(this);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestResourceResolverComponent(resolver)
                {
                    Priority = -i,
                    TryGetResource = (s, o, arg4) =>
                    {
                        ++invokeCount;
                        s.ShouldEqual(name);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return default;
                    }
                };
                resolver.AddComponent(component);
            }

            resolver.TryGetResource(name, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetTypeShouldBeHandledByComponents(int componentCount)
        {
            var resolver = new ResourceResolver();
            var name = "name";
            var request = this;
            var result = typeof(string);
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestTypeResolverComponent(resolver)
                {
                    Priority = -i,
                    TryGetType = (s, o, arg4) =>
                    {
                        ++invokeCount;
                        s.ShouldEqual(name);
                        o.ShouldEqual(request);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return result;
                        return null;
                    }
                };
                resolver.AddComponent(component);
            }

            resolver.TryGetType(name, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override ResourceResolver GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new ResourceResolver(collectionProvider);

        #endregion
    }
}