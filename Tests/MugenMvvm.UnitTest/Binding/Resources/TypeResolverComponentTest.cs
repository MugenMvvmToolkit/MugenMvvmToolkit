using MugenMvvm.Binding.Resources.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Resources
{
    public class TypeResolverComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetResourceValueShouldReturnNullEmpty()
        {
            var component = new TypeResolverComponent();
            component.TryGetType("test", this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            var resource = typeof(string);
            var component = new TypeResolverComponent();
            component.Types.Clear();

            component.AddType(resource);
            component.Types.Count.ShouldEqual(2);
            component.Types[resource.Name].ShouldEqual(resource);
            component.Types[resource.FullName].ShouldEqual(resource);
            component.TryGetType(resource.Name, this, DefaultMetadata).ShouldEqual(resource);
            component.TryGetType(resource.FullName, this, DefaultMetadata).ShouldEqual(resource);

            component.Types.Remove(resource.Name);
            component.Types.Remove(resource.FullName);
            component.TryGetType(resource.FullName, this, DefaultMetadata).ShouldBeNull();
            component.TryGetType(resource.Name, this, DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}