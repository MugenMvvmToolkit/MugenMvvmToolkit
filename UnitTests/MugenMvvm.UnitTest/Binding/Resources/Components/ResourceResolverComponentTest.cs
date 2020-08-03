using MugenMvvm.Binding.Resources.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Resources.Components
{
    public class ResourceResolverComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetResourceValueShouldReturnUnresolvedResource()
        {
            var component = new ResourceResolverComponent();
            component.TryGetResource(null!, "test", this, DefaultMetadata).IsResolved.ShouldBeFalse();
        }

        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            var name = "test";
            var resource = new object();
            var component = new ResourceResolverComponent();
            component.Resources.Count.ShouldEqual(0);

            component.Add(name, resource);
            component.Resources.Count.ShouldEqual(1);
            component.Resources[name].ShouldEqual(resource);
            component.TryGetResource(null!, name, this, DefaultMetadata).Resource.ShouldEqual(resource);

            component.Remove(name);
            component.TryGetResource(null!, name, this, DefaultMetadata).IsResolved.ShouldBeFalse();
        }

        #endregion
    }
}