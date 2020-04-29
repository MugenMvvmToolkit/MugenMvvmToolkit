using System.Linq;
using MugenMvvm.Binding.Observers.MemberPaths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class SingleMemberPathTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            const string path = "Path";
            var singleMemberPath = new SingleMemberPath(path);
            singleMemberPath.Members.Single().ShouldEqual(path);
            singleMemberPath.Members[0].ShouldEqual(path);
            singleMemberPath.Members.Count.ShouldEqual(1);
            singleMemberPath.Path.ShouldEqual(path);
        }

        #endregion
    }
}