using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ActionTokenTest : UnitTestBase
    {
        [Fact]
        public void DelegateShouldBeInvokedOnce3()
        {
            var count = 0;
            var actionToken = ActionToken.FromDelegate(() => ++count);
            actionToken.IsEmpty.ShouldBeFalse();
            actionToken.Dispose();
            actionToken.Dispose();
            count.ShouldEqual(1);
            actionToken.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void NoDoShouldNotBeEmpty() => ActionToken.NoDo.IsEmpty.ShouldBeFalse();

        [Fact]
        public void NoDoShouldReturnNewObject()
        {
            ActionToken.NoDo.Dispose();
            ActionToken.NoDo.IsEmpty.ShouldBeFalse();
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("1", null)]
        [InlineData(null, "1")]
        [InlineData("1", "2")]
        public void HandlerShouldBeInvokedOnce(object? state1, object? state2)
        {
            var count = 0;
            var testHandler = new TestHandler
            {
                Invoke = (o, o1) =>
                {
                    o.ShouldEqual(state1);
                    o1.ShouldEqual(state2);
                    ++count;
                }
            };
            var actionToken = ActionToken.FromHandler(testHandler, state1, state2);
            actionToken.IsEmpty.ShouldBeFalse();
            actionToken.Dispose();
            actionToken.Dispose();
            count.ShouldEqual(1);
            actionToken.IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("1", null)]
        [InlineData(null, "1")]
        [InlineData("1", "2")]
        public void DelegateShouldBeInvokedOnce1(object? state1, object? state2)
        {
            var count = 0;
            var actionToken = ActionToken.FromDelegate((o, o1) =>
            {
                o.ShouldEqual(state1);
                o1.ShouldEqual(state2);
                ++count;
            }, state1, state2);
            actionToken.IsEmpty.ShouldBeFalse();
            actionToken.Dispose();
            actionToken.Dispose();
            count.ShouldEqual(1);
            actionToken.IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("1")]
        public void DelegateShouldBeInvokedOnce2(string? state)
        {
            var count = 0;
            var actionToken = ActionToken.FromDelegate(state, s =>
            {
                s.ShouldEqual(state);
                ++count;
            });
            actionToken.IsEmpty.ShouldBeFalse();
            actionToken.Dispose();
            actionToken.Dispose();
            count.ShouldEqual(1);
            actionToken.IsEmpty.ShouldBeTrue();
        }
    }
}