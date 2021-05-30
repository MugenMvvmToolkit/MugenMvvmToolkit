using MugenMvvm.App;
using MugenMvvm.App.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Messaging;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.App.Components
{
    public class AppLifecycleTrackerTest : UnitTestBase
    {
        private readonly MugenApplication _application;
        private readonly Messenger _messenger;
        private readonly NavigationDispatcher _navigationDispatcher;

        public AppLifecycleTrackerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _application = new MugenApplication(null, ComponentCollectionManager);
            _navigationDispatcher = new NavigationDispatcher(ComponentCollectionManager);
            _navigationDispatcher.AddComponent(new NavigationContextProvider());
            _messenger = new Messenger(ComponentCollectionManager);
            _application.AddComponent(new AppLifecycleTracker(_navigationDispatcher, _messenger, AttachedValueManager));
        }

        [Fact]
        public void ShouldCallOnNavigatedActivatedDeactivated()
        {
            INavigationContext? ctx = null;
            using var t = _navigationDispatcher.AddComponent(new TestNavigationListener(_navigationDispatcher)
            {
                OnNavigated = context =>
                {
                    ctx.ShouldBeNull();
                    ctx = context;
                }
            });
            _application.OnLifecycleChanged(ApplicationLifecycleState.Initialized, null, DefaultMetadata);
            ctx.ShouldBeNull();

            _application.OnLifecycleChanged(ApplicationLifecycleState.Activated, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.Close);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(_application);

            ctx = null;
            _application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.New);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(_application);
        }

        [Fact]
        public void ShouldCallOnNavigatingActivatingDeactivating()
        {
            INavigationContext? ctx = null;
            using var t = _navigationDispatcher.AddComponent(new TestNavigationListener(_navigationDispatcher)
            {
                OnNavigating = context =>
                {
                    ctx.ShouldBeNull();
                    ctx = context;
                }
            });
            _application.OnLifecycleChanged(ApplicationLifecycleState.Initialized, null, DefaultMetadata);
            ctx.ShouldBeNull();

            _application.OnLifecycleChanged(ApplicationLifecycleState.Activating, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.Close);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(_application);

            ctx = null;
            _application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, null, DefaultMetadata);
            ctx!.NavigationMode.ShouldEqual(NavigationMode.New);
            ctx.NavigationId.ShouldEqual(InternalConstant.BackgroundNavigationId);
            ctx.NavigationType.ShouldEqual(NavigationType.Background);
            ctx.NavigationProvider.ShouldEqual(NavigationProvider.System);
            ctx.Target.ShouldEqual(_application);
        }

        [Fact]
        public void ShouldChangeAppStateActivatedDeactivated()
        {
            _application.IsInState(ApplicationLifecycleState.Activated).ShouldBeFalse();

            _application.OnLifecycleChanged(ApplicationLifecycleState.Activated, null, DefaultMetadata);
            _application.IsInState(ApplicationLifecycleState.Activated).ShouldBeTrue();
            _application.IsInState(ApplicationLifecycleState.Deactivated).ShouldBeFalse();

            _application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null, DefaultMetadata);
            _application.IsInState(ApplicationLifecycleState.Activated).ShouldBeFalse();
            _application.IsInState(ApplicationLifecycleState.Deactivated).ShouldBeTrue();
        }

        [Fact]
        public void ShouldPublishStateMessage()
        {
            var state = ApplicationLifecycleState.Initialized;
            var invokeCount = 0;
            using var t = _messenger.AddComponent(new TestMessagePublisherComponent(null)
            {
                TryPublish = context =>
                {
                    ++invokeCount;
                    context.Sender.ShouldEqual(_application);
                    context.Message.ShouldEqual(state);
                    return true;
                }
            });

            _application.OnLifecycleChanged(state, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            state = ApplicationLifecycleState.Activated;
            _application.OnLifecycleChanged(state, null, DefaultMetadata);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldRegisterBackgroundCallback()
        {
            var invokeCount = 0;
            var callbackType = NavigationCallbackType.Showing;
            using var t = _navigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryAddNavigationCallback = (type, s, arg3, arg4, arg5) =>
                {
                    type.ShouldEqual(callbackType);
                    s.ShouldEqual(InternalConstant.BackgroundNavigationId);
                    arg3.ShouldEqual(NavigationType.Background);
                    arg4.ShouldEqual(_application);
                    arg5.ShouldEqual(DefaultMetadata);
                    invokeCount++;
                    return null;
                }
            });
            _application.OnLifecycleChanged(ApplicationLifecycleState.Activated, null, DefaultMetadata);

            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            callbackType = NavigationCallbackType.Close;
            _application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }
    }
}