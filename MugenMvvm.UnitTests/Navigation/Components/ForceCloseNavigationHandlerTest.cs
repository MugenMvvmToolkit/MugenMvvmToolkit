using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class ForceCloseNavigationHandlerTest : UnitTestBase
    {
        private readonly NavigationDispatcher _dispatcher;

        public ForceCloseNavigationHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _dispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _dispatcher.AddComponent(new ForceCloseNavigationHandler());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanNavigateAsyncShouldCheckForceCloseValue(bool value)
        {
            var invokeCount = 0;
            var ctx = new NavigationContext(this, NavigationProvider.System, "0", NavigationType.Popup, NavigationMode.Close, NavigationMetadata.ForceClose.ToContext(value));
            _dispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (context, token) =>
                {
                    context.ShouldEqual(ctx);
                    token.ShouldEqual(DefaultCancellationToken);
                    ++invokeCount;
                    return new ValueTask<bool>(false);
                }
            });

            (await _dispatcher.OnNavigatingAsync(ctx, DefaultCancellationToken)).ShouldEqual(value);
            invokeCount.ShouldEqual(value ? 0 : 1);
        }
    }
}