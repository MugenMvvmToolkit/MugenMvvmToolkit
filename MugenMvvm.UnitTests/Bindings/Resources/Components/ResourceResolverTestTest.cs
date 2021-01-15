using MugenMvvm.Bindings.Resources.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Resources.Components
{
    public class ResourceResolverTestTest : UnitTestBase
    {
        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            var name = "test";
            var resource = new object();
            var component = new ResourceResolver();
            component.Resources.Count.ShouldEqual(0);

            component.Add(name, resource);
            component.Resources.Count.ShouldEqual(1);
            component.Resources[name].ShouldEqual(resource);
            component.TryGetResource(null!, name, this, DefaultMetadata).Resource.ShouldEqual(resource);

            component.Remove(name);
            component.TryGetResource(null!, name, this, DefaultMetadata).IsResolved.ShouldBeFalse();
        }

        [Fact]
        public void TryGetResourceValueShouldReturnUnresolvedResource()
        {
            var component = new ResourceResolver();
            component.TryGetResource(null!, "test", this, DefaultMetadata).IsResolved.ShouldBeFalse();
        }
    }
}