using MugenMvvm.Binding.Observation.Paths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation.Paths
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