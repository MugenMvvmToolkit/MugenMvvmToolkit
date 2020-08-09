using System;
using System.Linq;
using MugenMvvm.Binding.Observation.Paths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation.Paths
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
            ShouldThrow<ArgumentOutOfRangeException>(() =>
            {
                var member = singleMemberPath.Members[1];
            });
        }

        #endregion
    }
}