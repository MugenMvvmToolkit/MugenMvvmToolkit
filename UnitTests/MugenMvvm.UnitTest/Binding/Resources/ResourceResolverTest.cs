using MugenMvvm.Binding.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Resources.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Resources
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
            var result = new TestResourceValue();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestResourceResolverComponent(resolver)
                {
                    Priority = -i,
                    TryGetResourceValue = (s, o, arg4) =>
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

            resolver.TryGetResourceValue(name, request, DefaultMetadata).ShouldEqual(result);
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

        protected override ResourceResolver GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new ResourceResolver(collectionProvider);
        }

        #endregion
    }
}