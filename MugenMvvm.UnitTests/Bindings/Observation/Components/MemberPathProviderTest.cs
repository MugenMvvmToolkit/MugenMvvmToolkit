using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class MemberPathProviderTest : UnitTestBase
    {
        [Fact]
        public void TryGetMemberPathShouldReturnEmptyPath()
        {
            var component = new MemberPathProvider();
            component.TryGetMemberPath(null!, "", DefaultMetadata).ShouldEqual(MemberPath.Empty);
        }

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyUnsupportedRequest()
        {
            var component = new MemberPathProvider();
            component.TryGetMemberPath(null!, this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetMemberPathShouldReturnMultiPath()
        {
            const string member = "Test.Test[T]";
            var component = new MemberPathProvider();
            var path = component.TryGetMemberPath(null!, member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
            path.Members.AsList().ShouldEqual(new[] {"Test", "Test", "[T]"});
        }

        [Fact]
        public void TryGetMemberPathShouldReturnSinglePath()
        {
            const string member = "Test";
            var component = new MemberPathProvider();
            var path = component.TryGetMemberPath(null!, member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
            path.Members.Item.ShouldEqual(member);
        }
    }
}