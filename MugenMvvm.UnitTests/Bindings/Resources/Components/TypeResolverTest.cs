using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Resources.Components
{
    public class TypeResolverTest : UnitTestBase
    {
        private readonly TypeResolver _resolver;

        public TypeResolverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _resolver = new TypeResolver();
            ResourceManager.AddComponent(_resolver);
        }

        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            string alias = "aa";
            var resource = typeof(string);

            _resolver.Types.Clear();

            _resolver.AddType(resource, alias);
            _resolver.Types.Count.ShouldEqual(3);
            _resolver.Types[alias].ShouldEqual(resource);
            _resolver.Types[resource.Name].ShouldEqual(resource);
            _resolver.Types[resource.FullName!].ShouldEqual(resource);
            ResourceManager.TryGetType(resource.Name, this, DefaultMetadata).ShouldEqual(resource);
            ResourceManager.TryGetType(resource.FullName!, this, DefaultMetadata).ShouldEqual(resource);
            ResourceManager.TryGetType(alias, this, DefaultMetadata).ShouldEqual(resource);

            _resolver.Types.Remove(resource.Name);
            _resolver.Types.Remove(resource.FullName!);
            _resolver.Types.Remove(alias);
            ResourceManager.TryGetType(resource.FullName!, this, DefaultMetadata).ShouldBeNull();
            ResourceManager.TryGetType(resource.Name, this, DefaultMetadata).ShouldBeNull();
            ResourceManager.TryGetType(alias, this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetResourceValueShouldReturnNullEmpty() => ResourceManager.TryGetType("test", this, DefaultMetadata).ShouldBeNull();

        protected override IResourceManager GetResourceManager() => new ResourceManager(ComponentCollectionManager);
    }
}