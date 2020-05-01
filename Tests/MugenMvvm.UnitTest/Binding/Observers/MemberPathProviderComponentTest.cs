using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.Binding.Observers.MemberPaths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MemberPathProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyUnsupportedRequest()
        {
            var component = new MemberPathProviderComponent();
            component.TryGetMemberPath(this, DefaultMetadata).ShouldBeNull();
        }

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyPath()
        {
            var component = new MemberPathProviderComponent();
            component.TryGetMemberPath("", DefaultMetadata).ShouldEqual(EmptyMemberPath.Instance);
        }

        [Fact]
        public void TryGetMemberPathShouldReturnSinglePath()
        {
            const string member = "Test";
            var component = new MemberPathProviderComponent();
            var path = (SingleMemberPath) component.TryGetMemberPath(member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
        }

        [Fact]
        public void TryGetMemberPathShouldReturnMultiPath()
        {
            const string member = "Test.Test[T]";
            var component = new MemberPathProviderComponent();
            var path = (MultiMemberPath) component.TryGetMemberPath(member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
        }

        #endregion
    }
}