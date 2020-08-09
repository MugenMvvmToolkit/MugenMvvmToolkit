using MugenMvvm.App;
using MugenMvvm.App.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.App.Components
{
    public class AppBackgroundDispatcherTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldCallOnNavigatingActivatingDeactivating()
        {
            INavigationContext? ctx = null;
            var app = new MugenApplication();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatingListener(navigationDispatcher)
            {
                OnNavigating = context =>
                {
                    ctx.ShouldBeNull();
                    ctx = context;
                }
            });
            var dispatcher = new AppBackgroundDispatcher(navigationDispatcher);
            dispatcher.OnLifecycleChanged(null!, ApplicationLifecycleState.Initialized, null, DefaultMetadata);
            ctx.ShouldBeNull();

            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Activating, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.Close);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(Default.NavigationProvider);
            ctx.Target.ShouldEqual(app);

            ctx = null;
            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Deactivating, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.New);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(Default.NavigationProvider);
            ctx.Target.ShouldEqual(app);
        }

        [Fact]
        public void ShouldCallOnNavigatedActivatedDeactivated()
        {
            INavigationContext? ctx = null;
            var app = new MugenApplication();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationDispatcherNavigatedListener(navigationDispatcher)
            {
                OnNavigated = context =>
                {
                    ctx.ShouldBeNull();
                    ctx = context;
                }
            });
            var dispatcher = new AppBackgroundDispatcher(navigationDispatcher);
            dispatcher.OnLifecycleChanged(null!, ApplicationLifecycleState.Initialized, null, DefaultMetadata);
            ctx.ShouldBeNull();

            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Activated, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.Close);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(Default.NavigationProvider);
            ctx.Target.ShouldEqual(app);

            ctx = null;
            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Deactivated, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.New);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(Default.NavigationProvider);
            ctx.Target.ShouldEqual(app);
        }

        [Fact]
        public void ShouldChangeAppStateActivatedDeactivated()
        {
            var app = new MugenApplication();
            var navigationDispatcher = new NavigationDispatcher();
            var dispatcher = new AppBackgroundDispatcher(navigationDispatcher);
            app.IsInBackground().ShouldEqual(false);
            app.IsInBackground(true).ShouldEqual(true);

            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Activated, null, DefaultMetadata);
            app.IsInBackground(true).ShouldEqual(false);

            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Deactivated, null, DefaultMetadata);
            app.IsInBackground().ShouldEqual(true);
        }

        [Fact]
        public void ShouldRegisterBackgroundCallback()
        {
            var invokeCount = 0;
            var callbackType = NavigationCallbackType.Showing;
            var app = new MugenApplication();
            var navigationDispatcher = new NavigationDispatcher();
            navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (type, s, arg3, arg4, arg5) =>
                {
                    type.ShouldEqual(callbackType);
                    s.ShouldEqual(InternalConstant.BackgroundNavigationId);
                    arg3.ShouldEqual(NavigationType.Background);
                    arg4.ShouldEqual(app);
                    arg5.ShouldEqual(DefaultMetadata);
                    invokeCount++;
                    return null;
                }
            });
            var dispatcher = new AppBackgroundDispatcher(navigationDispatcher);
            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Activated, null, DefaultMetadata);

            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            callbackType = NavigationCallbackType.Close;
            dispatcher.OnLifecycleChanged(app, ApplicationLifecycleState.Deactivated, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}