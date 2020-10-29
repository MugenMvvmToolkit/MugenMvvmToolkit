using System;
using System.Linq;
using MugenMvvm.Bindings.Observation.Paths;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Paths
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