using MugenMvvm.Bindings.Observation.Paths;
using MugenMvvm.Interfaces.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Paths
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
            var valueHolder = (IValueHolder<string>) emptyMemberPath;
            valueHolder.Value = nameof(valueHolder);
            valueHolder.Value.ShouldEqual(nameof(valueHolder));
        }

        #endregion
    }
}