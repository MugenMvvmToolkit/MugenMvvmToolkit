using Should;
using Xunit;

namespace MugenMvvm.UnitTest
{
    public class ActionTokenTest : UnitTestBase
    {
        #region Methods

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
            var actionToken = new ActionToken(testHandler, state1, state2);
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
        public void DelegateShouldBeInvokedOnce(object? state1, object? state2)
        {
            var count = 0;
            var actionToken = new ActionToken((o, o1) =>
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

        [Fact]
        public void NoDoShouldNotBeEmpty()
        {
            ActionToken.NoDoToken.IsEmpty.ShouldBeFalse();
        }

        [Fact]
        public void NoDoShouldReturnNewObject()
        {
            ActionToken.NoDoToken.Dispose();
            ActionToken.NoDoToken.IsEmpty.ShouldBeFalse();
        }

        #endregion
    }
}