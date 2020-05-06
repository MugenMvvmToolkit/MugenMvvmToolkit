using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.Binding.Observers.MemberPaths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.Components
{
    public class MemberPathProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyUnsupportedRequest()
        {
            var component = new MemberPathProvider();
            component.TryGetMemberPath(this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyPath()
        {
            var component = new MemberPathProvider();
            component.TryGetMemberPath("", DefaultMetadata).ShouldEqual(EmptyMemberPath.Instance);
        }

        [Fact]
        public void TryGetMemberPathShouldReturnSinglePath()
        {
            const string member = "Test";
            var component = new MemberPathProvider();
            var path = (SingleMemberPath) component.TryGetMemberPath(member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
        }

        [Fact]
        public void TryGetMemberPathShouldReturnMultiPath()
        {
            const string member = "Test.Test[T]";
            var component = new MemberPathProvider();
            var path = (MultiMemberPath) component.TryGetMemberPath(member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
        }

        #endregion
    }
}