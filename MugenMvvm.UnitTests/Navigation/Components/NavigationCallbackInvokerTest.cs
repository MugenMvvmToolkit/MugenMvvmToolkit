using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTests.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationCallbackInvokerTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnNavigatedShouldInvokeCallbacks(bool close)
        {
            var navigationContext = new NavigationContext(this, NavigationProvider.System, "t", NavigationType.Popup, close ? NavigationMode.Close : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (callbackType, ctx, ct) => throw new NotSupportedException(),
                TryInvokeExceptionNavigationCallbacks = (callbackType, ctx, ex) => throw new NotSupportedException(),
                TryInvokeNavigationCallbacks = (callbackType, ctx) =>
                {
                    callbackTypes.Add(callbackType);
                    ctx.ShouldEqual(navigationContext);
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
            var navigationContext = new NavigationContext(this, NavigationProvider.System, "t", NavigationType.Popup, close ? NavigationMode.Close : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (callbackType, ctx, ct) => throw new NotSupportedException(),
                TryInvokeExceptionNavigationCallbacks = (callbackType, ctx, ex) =>
                {
                    callbackTypes.Add(callbackType);
                    ctx.ShouldEqual(navigationContext);
                    ex.ShouldEqual(exception);
                    return true;
                },
                TryInvokeNavigationCallbacks = (callbackType, ctx) => throw new NotSupportedException()
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
            var navigationContext = new NavigationContext(this, NavigationProvider.System, "t", NavigationType.Popup, close ? NavigationMode.Close : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            var dispatcher = new NavigationDispatcher();
            dispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (callbackType, ctx, ct) =>
                {
                    callbackTypes.Add(callbackType);
                    ctx.ShouldEqual(navigationContext);
                    ct.ShouldEqual(token);
                    return true;
                },
                TryInvokeExceptionNavigationCallbacks = (callbackType, ctx, e) => throw new NotSupportedException(),
                TryInvokeNavigationCallbacks = (callbackType, ctx) => throw new NotSupportedException()
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