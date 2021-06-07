using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Resources.Components
{
    public class ResourceResolverTestTest : UnitTestBase
    {
        private readonly ResourceResolver _resolver;

        public ResourceResolverTestTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _resolver = new ResourceResolver();
            ResourceManager.AddComponent(_resolver);
        }

        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            var name = "test";
            var resource = new object();

            _resolver.Resources.Count.ShouldEqual(0);

            _resolver.Add(name, resource);
            _resolver.Resources.Count.ShouldEqual(1);
            _resolver.Resources[name].ShouldEqual(resource);
            ResourceManager.TryGetResource(name, this, DefaultMetadata).Resource.ShouldEqual(resource);

            _resolver.Remove(name);
            ResourceManager.TryGetResource(name, this, DefaultMetadata).IsResolved.ShouldBeFalse();
        }

        [Fact]
        public void TryGetResourceValueShouldReturnUnresolvedResource() => ResourceManager.TryGetResource("test", this, DefaultMetadata).IsResolved.ShouldBeFalse();

        protected override IResourceManager GetResourceManager() => new ResourceManager(ComponentCollectionManager);
    }
}