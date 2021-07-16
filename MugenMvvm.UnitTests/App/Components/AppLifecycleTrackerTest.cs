using MugenMvvm.App;
using MugenMvvm.App.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.Navigation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.App.Components
{
    public class AppLifecycleTrackerTest : UnitTestBase
    {
        public AppLifecycleTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            NavigationDispatcher.AddComponent(new NavigationContextProvider());
        }

        [Fact]
        public void ShouldCallOnNavigatedActivatedDeactivated()
        {
            INavigationContext? ctx = null;
            using var t = NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigated = (_, context) =>
                {
                    ctx.ShouldBeNull();
                    ctx = context;
                }
            });
            Application.OnLifecycleChanged(ApplicationLifecycleState.Initialized, null, Metadata);
            ctx.ShouldBeNull();

            Application.OnLifecycleChanged(ApplicationLifecycleState.Activated, null, Metadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.Close);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(Application);

            ctx = null;
            Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null, Metadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.New);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(Application);
        }

        [Fact]
        public void ShouldCallOnNavigatingActivatingDeactivating()
        {
            INavigationContext? ctx = null;
            using var t = NavigationDispatcher.AddComponent(new TestNavigationListener
            {
                OnNavigating = (_, context) =>
                {
                    ctx.ShouldBeNull();
                    ctx = context;
                }
            });
            Application.OnLifecycleChanged(ApplicationLifecycleState.Initialized, null, Metadata);
            ctx.ShouldBeNull();

            Application.OnLifecycleChanged(ApplicationLifecycleState.Activating, null, Metadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.Close);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(Application);

            ctx = null;
            Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, null, Metadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.New);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(Application);
        }

        [Fact]
        public void ShouldChangeAppStateActivatedDeactivated()
        {
            Application.IsInState(ApplicationLifecycleState.Activated).ShouldBeFalse();

            Application.OnLifecycleChanged(ApplicationLifecycleState.Activated, null, Metadata);
            Application.IsInState(ApplicationLifecycleState.Activated).ShouldBeTrue();
            Application.IsInState(ApplicationLifecycleState.Deactivated).ShouldBeFalse();

            Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null, Metadata);
            Application.IsInState(ApplicationLifecycleState.Activated).ShouldBeFalse();
            Application.IsInState(ApplicationLifecycleState.Deactivated).ShouldBeTrue();
        }

        [Fact]
        public void ShouldPublishStateMessage()
        {
            var state = ApplicationLifecycleState.Initialized;
            var invokeCount = 0;
            using var t = Messenger.AddComponent(new TestMessagePublisherComponent
            {
                TryPublish = (_, ctx) =>
                {
                    ++invokeCount;
                    ctx.Sender.ShouldEqual(Application);
                    ctx.Message.ShouldEqual(state);
                    return true;
                }
            });

            Application.OnLifecycleChanged(state, null, Metadata);
            invokeCount.ShouldEqual(1);

            state = ApplicationLifecycleState.Activated;
            Application.OnLifecycleChanged(state, null, Metadata);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldRegisterBackgroundCallback()
        {
            var invokeCount = 0;
            var callbackType = NavigationCallbackType.Showing;
            using var t = NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (_, type, s, arg3, arg4, arg5) =>
                {
                    type.ShouldEqual(callbackType);
                    s.ShouldEqual(InternalConstant.BackgroundNavigationId);
                    arg3.ShouldEqual(NavigationType.Background);
                    arg4.ShouldEqual(Application);
                    arg5.ShouldEqual(Metadata);
                    invokeCount++;
                    return null;
                }
            });
            Application.OnLifecycleChanged(ApplicationLifecycleState.Activated, null, Metadata);

            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            callbackType = NavigationCallbackType.Close;
            Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null, Metadata);
            invokeCount.ShouldEqual(1);
        }

        protected override IMugenApplication GetApplication()
        {
            var app = new MugenApplication(null, ComponentCollectionManager);
            app.AddComponent(new AppLifecycleTracker(NavigationDispatcher, Messenger, AttachedValueManager));
            return app;
        }
    }
}