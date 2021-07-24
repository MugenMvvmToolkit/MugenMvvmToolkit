using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class DisposeCallbackComponentTest : UnitTestBase
    {
        public DisposeCallbackComponentTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void OnDetachingShouldBeFalse()
        {
            var component = Command.GetOrAddComponent<DisposeCallbackComponent<ICompositeCommand>>();
            Command.ClearComponents();
            Command.GetComponent<DisposeCallbackComponent<ICompositeCommand>>().ShouldEqual(component);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RegisterDisposeTokenShouldInvokeTokenOnDispose(int count)
        {
            var invokedCount = 0;
            for (var i = 0; i < count; i++)
                Command.RegisterDisposeToken(ActionToken.FromDelegate((s1, s2) => ++invokedCount));

            invokedCount.ShouldEqual(0);
            Command.Dispose();
            invokedCount.ShouldEqual(count);

            Command.Dispose();
            invokedCount.ShouldEqual(count);

            invokedCount = 0;
            for (var i = 0; i < count; i++)
                Command.RegisterDisposeToken(ActionToken.FromDelegate((s1, s2) => ++invokedCount));
            invokedCount.ShouldEqual(count);
        }
    }
}