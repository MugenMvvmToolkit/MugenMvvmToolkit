using System.Linq;
using MugenMvvm.Binding.Observation.Paths;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Observation.Paths
{
    public class MultiMemberPathTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            const string path = "[test]";
            var memberPath = new MultiMemberPath(path);
            memberPath.Members.Single().ShouldEqual(path);
            memberPath.Path.ShouldEqual(path);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var strings = new[] {"Test", "Test1", "Test2", "Test3"};
            string path = string.Join(".", strings);
            var memberPath = new MultiMemberPath(path);
            memberPath.Members.ShouldEqual(strings);
            memberPath.Path.ShouldEqual(path);
        }

        [Fact]
        public void ConstructorShouldInitializeValues3()
        {
            var strings = new[] {"[test]", "Test1", "Test2[test]", "Test3"};
            var items = new[] {"[test]", "Test1", "Test2", "[test]", "Test3"};
            string path = string.Join(".", strings);
            var memberPath = new MultiMemberPath(path);
            memberPath.Members.ShouldEqual(items);
            memberPath.Path.ShouldEqual(path);
        }

        [Fact]
        public void ConstructorShouldInitializeValues4()
        {
            var strings = new[] {"[test, test2] ", "Test1 ", "Test2  [test , 10] ", "Test3 "};
            var items = new[] {"[test, test2]", "Test1", "Test2", "[test , 10]", "Test3"};
            string path = string.Join(".", strings);
            var memberPath = new MultiMemberPath(path);
            memberPath.Members.ShouldEqual(items);
        }

        #endregion
    }
}