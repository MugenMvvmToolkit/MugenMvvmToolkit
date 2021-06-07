using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Navigation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class ForceCloseNavigationHandlerTest : UnitTestBase
    {
        public ForceCloseNavigationHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            NavigationDispatcher.AddComponent(new ForceCloseNavigationHandler());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanNavigateAsyncShouldCheckForceCloseValue(bool value)
        {
            var invokeCount = 0;
            var ctx = GetNavigationContext(this, metadata: NavigationMetadata.ForceClose.ToContext(value));
            NavigationDispatcher.AddComponent(new TestNavigationConditionComponent
            {
                CanNavigateAsync = (d, context, token) =>
                {
                    d.ShouldEqual(NavigationDispatcher);
                    context.ShouldEqual(ctx);
                    token.ShouldEqual(DefaultCancellationToken);
                    ++invokeCount;
                    return new ValueTask<bool>(false);
                }
            });

            (await NavigationDispatcher.OnNavigatingAsync(ctx, DefaultCancellationToken)).ShouldEqual(value);
            invokeCount.ShouldEqual(value ? 0 : 1);
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);
    }
}