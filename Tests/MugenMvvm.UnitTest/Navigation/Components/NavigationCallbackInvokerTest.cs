using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation.Components
{
    public class NavigationCallbackInvokerTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnNavigatedShouldInvokeCallbacks(bool close)
        {
            var navigationContext = new NavigationContext(Default.NavigationProvider, "t", NavigationType.Popup, close ? NavigationMode.Back : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (type, o, arg3, arg4, arg5) => throw new NotSupportedException(),
                TryInvokeExceptionNavigationCallbacks = (type, o, arg3, arg4, arg5) => throw new NotSupportedException(),
                TryInvokeNavigationCallbacks = (callbackType, target, t, m) =>
                {
                    callbackTypes.Add(callbackType);
                    target.ShouldEqual(navigationContext);
                    t.ShouldEqual(typeof(INavigationContext));
                    m.ShouldEqual(navigationContext.GetMetadataOrDefault());
                    return true;
                }
            });
            var callbackInvoker = new NavigationCallbackInvoker();
            callbackInvoker.IsSuspended.ShouldBeFalse();
            dispatcher.AddComponent(callbackInvoker);
            var actionToken = dispatcher.GetComponents<ISuspendable>().Suspend(this, null);
            callbackInvoker.IsSuspended.ShouldBeTrue();
            dispatcher.OnNavigated(navigationContext);

            callbackTypes.Count.ShouldEqual(0);
            actionToken.Dispose();
            callbackInvoker.IsSuspended.ShouldBeFalse();
            if (close)
            {
                callbackTypes.Count.ShouldEqual(3);
                callbackTypes.ShouldContain(NavigationCallbackType.Close);
                callbackTypes.ShouldContain(NavigationCallbackType.Closing);
                callbackTypes.ShouldContain(NavigationCallbackType.Showing);
            }
            else
            {
                callbackTypes.Count.ShouldEqual(1);
                callbackTypes.ShouldContain(NavigationCallbackType.Showing);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnNavigationFailedShouldInvokeCallbacks(bool close)
        {
            var exception = new Exception();
            var navigationContext = new NavigationContext(Default.NavigationProvider, "t", NavigationType.Popup, close ? NavigationMode.Back : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (type, o, arg3, arg4, arg5) => throw new NotSupportedException(),
                TryInvokeExceptionNavigationCallbacks = (callbackType, target, t, e, m) =>
                {
                    callbackTypes.Add(callbackType);
                    target.ShouldEqual(navigationContext);
                    t.ShouldEqual(typeof(INavigationContext));
                    e.ShouldEqual(exception);
                    m.ShouldEqual(navigationContext.GetMetadataOrDefault());
                    return true;
                },
                TryInvokeNavigationCallbacks = (callbackType, target, t, m) => throw new NotSupportedException()
            });
            var callbackInvoker = new NavigationCallbackInvoker();
            callbackInvoker.IsSuspended.ShouldBeFalse();
            dispatcher.AddComponent(callbackInvoker);
            var actionToken = dispatcher.GetComponents<ISuspendable>().Suspend(this, null);
            callbackInvoker.IsSuspended.ShouldBeTrue();
            dispatcher.OnNavigationFailed(navigationContext, exception);

            callbackTypes.Count.ShouldEqual(0);
            actionToken.Dispose();
            callbackInvoker.IsSuspended.ShouldBeFalse();
            callbackTypes.Count.ShouldEqual(3);
            callbackTypes.ShouldContain(NavigationCallbackType.Close);
            callbackTypes.ShouldContain(NavigationCallbackType.Closing);
            callbackTypes.ShouldContain(NavigationCallbackType.Showing);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnNavigationCanceledShouldInvokeCallbacks(bool close)
        {
            var token = new CancellationTokenSource().Token;
            var navigationContext = new NavigationContext(Default.NavigationProvider, "t", NavigationType.Popup, close ? NavigationMode.Back : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (callbackType, target, t, ct, m) =>
                {
                    callbackTypes.Add(callbackType);
                    target.ShouldEqual(navigationContext);
                    t.ShouldEqual(typeof(INavigationContext));
                    ct.ShouldEqual(token);
                    m.ShouldEqual(navigationContext.GetMetadataOrDefault());
                    return true;
                },
                TryInvokeExceptionNavigationCallbacks = (callbackType, target, t, e, m) => throw new NotSupportedException(),
                TryInvokeNavigationCallbacks = (callbackType, target, t, m) => throw new NotSupportedException()
            });
            var callbackInvoker = new NavigationCallbackInvoker();
            callbackInvoker.IsSuspended.ShouldBeFalse();
            dispatcher.AddComponent(callbackInvoker);
            var actionToken = dispatcher.GetComponents<ISuspendable>().Suspend(this, null);
            callbackInvoker.IsSuspended.ShouldBeTrue();
            dispatcher.OnNavigationCanceled(navigationContext, token);

            callbackTypes.Count.ShouldEqual(0);
            actionToken.Dispose();
            callbackInvoker.IsSuspended.ShouldBeFalse();
            if (close)
            {
                callbackTypes.Count.ShouldEqual(1);
                callbackTypes.ShouldContain(NavigationCallbackType.Closing);
            }
            else
            {
                callbackTypes.Count.ShouldEqual(3);
                callbackTypes.ShouldContain(NavigationCallbackType.Close);
                callbackTypes.ShouldContain(NavigationCallbackType.Closing);
                callbackTypes.ShouldContain(NavigationCallbackType.Showing);
            }
        }

        #endregion
    }
}