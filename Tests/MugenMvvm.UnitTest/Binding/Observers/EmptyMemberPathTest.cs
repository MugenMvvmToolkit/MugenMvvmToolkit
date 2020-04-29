using MugenMvvm.Binding.Observers.MemberPaths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class EmptyMemberPathTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InstanceShouldBeInitialized()
        {
            var emptyMemberPath = EmptyMemberPath.Instance;
            emptyMemberPath.Members.ShouldBeEmpty();
            emptyMemberPath.Path.ShouldEqual("");
        }

        #endregion
    }
}