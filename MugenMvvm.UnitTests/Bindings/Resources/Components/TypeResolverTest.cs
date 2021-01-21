﻿using MugenMvvm.Bindings.Resources.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Resources.Components
{
    public class TypeResolverTest : UnitTestBase
    {
        [Fact]
        public void TryGetResourceValueAddRemoveResource()
        {
            string alias = "aa";
            var resource = typeof(string);
            var component = new TypeResolver();
            component.Types.Clear();

            component.AddType(resource, alias);
            component.Types.Count.ShouldEqual(3);
            component.Types[alias].ShouldEqual(resource);
            component.Types[resource.Name].ShouldEqual(resource);
            component.Types[resource.FullName!].ShouldEqual(resource);
            component.TryGetType(null!, resource.Name, this, DefaultMetadata).ShouldEqual(resource);
            component.TryGetType(null!, resource.FullName!, this, DefaultMetadata).ShouldEqual(resource);
            component.TryGetType(null!, alias, this, DefaultMetadata).ShouldEqual(resource);

            component.Types.Remove(resource.Name);
            component.Types.Remove(resource.FullName!);
            component.Types.Remove(alias);
            component.TryGetType(null!, resource.FullName!, this, DefaultMetadata).ShouldBeNull();
            component.TryGetType(null!, resource.Name, this, DefaultMetadata).ShouldBeNull();
            component.TryGetType(null!, alias, this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetResourceValueShouldReturnNullEmpty()
        {
            var component = new TypeResolver();
            component.TryGetType(null!, "test", this, DefaultMetadata).ShouldBeNull();
        }
    }
}