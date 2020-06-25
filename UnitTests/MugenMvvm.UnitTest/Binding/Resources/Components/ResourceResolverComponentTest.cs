using MugenMvvm.Binding.Resources.Components;
using MugenMvvm.UnitTest.Binding.Resources.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Resources.Components
{
    public class ResourceResolverComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetResourceValueShouldReturnNullEmpty()
        {
            var component = new ResourceResolverComponent();
            component.TryGetResourceValue("test", this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            var name = "test";
            var resource = new TestResourceValue();
            var component = new ResourceResolverComponent();
            component.Resources.Count.ShouldEqual(0);

            component.AddResource(name, resource);
            component.Resources.Count.ShouldEqual(1);
            component.Resources[name].ShouldEqual(resource);
            component.TryGetResourceValue(name, this, DefaultMetadata).ShouldEqual(resource);

            component.Resources.Remove(name);
            component.TryGetResourceValue(name, this, DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}