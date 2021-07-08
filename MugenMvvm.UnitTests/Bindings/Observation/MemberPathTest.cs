using System;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation
{
    public class MemberPathTest : UnitTestBase
    {
        [Fact]
        public void ShouldInitializeValues1()
        {
            const string path = "[test]";
            var memberPath = MemberPath.Get(path);
            memberPath.Members.Item.ShouldEqual(path);
            memberPath.Path.ShouldEqual(path);
        }

        [Fact]
        public void ShouldInitializeValues2()
        {
            var strings = new[] { "Test", "Test1", "Test2", "Test3" };
            string path = string.Join(".", strings);
            var memberPath = MemberPath.Get(path);
            memberPath.Members.ShouldEqual(strings);
            memberPath.Path.ShouldEqual(path);
        }

        [Fact]
        public void ShouldInitializeValues3()
        {
            var strings = new[] { "[test]", "Test1", "Test2[test]", "Test3" };
            var items = new[] { "[test]", "Test1", "Test2", "[test]", "Test3" };
            string path = string.Join(".", strings);
            var memberPath = MemberPath.Get(path);
            memberPath.Members.ShouldEqual(items);
            memberPath.Path.ShouldEqual(path);
        }

        [Fact]
        public void ShouldInitializeValues4()
        {
            var strings = new[] { "[test, test2] ", "Test1 ", "Test2  [test , 10] ", "Test3 " };
            var items = new[] { "[test, test2]", "Test1", "Test2", "[test , 10]", "Test3" };
            string path = string.Join(".", strings);
            var memberPath = MemberPath.Get(path);
            memberPath.Path.ShouldEqual(path);
            memberPath.Members.ShouldEqual(items);
        }

        [Fact]
        public void ShouldInitializeValuesEmptyPath()
        {
            var emptyMemberPath = MemberPath.Get("");
            MemberPath.Empty.ShouldEqual(emptyMemberPath);
            emptyMemberPath.Members.IsEmpty.ShouldBeTrue();
            emptyMemberPath.Path.ShouldEqual("");
            var valueHolder = (IValueHolder<string>)emptyMemberPath;
            valueHolder.Value = nameof(valueHolder);
            valueHolder.Value.ShouldEqual(nameof(valueHolder));
        }

        [Fact]
        public void ShouldInitializeValuesSinglePath()
        {
            const string path = "Path";
            var singleMemberPath = MemberPath.Get(path);
            singleMemberPath.Members.Item.ShouldEqual(path);
            singleMemberPath.Members[0].ShouldEqual(path);
            singleMemberPath.Members.Count.ShouldEqual(1);
            singleMemberPath.Path.ShouldEqual(path);
            ShouldThrow<ArgumentOutOfRangeException>(() =>
            {
                var member = singleMemberPath.Members[1];
            });
        }
    }
}