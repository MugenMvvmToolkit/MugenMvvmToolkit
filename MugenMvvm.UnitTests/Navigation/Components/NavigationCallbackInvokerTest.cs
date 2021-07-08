using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Navigation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationCallbackInvokerTest : UnitTestBase
    {
        private readonly NavigationCallbackInvoker _callbackInvoker;

        public NavigationCallbackInvokerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _callbackInvoker = new NavigationCallbackInvoker();
            NavigationDispatcher.AddComponent(_callbackInvoker);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnNavigatedShouldInvokeCallbacks(bool close)
        {
            var navigationContext = GetNavigationContext(this, close ? NavigationMode.Close : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (_, _, _, _) => throw new NotSupportedException(),
                TryInvokeExceptionNavigationCallbacks = (_, _, _, _) => throw new NotSupportedException(),
                TryInvokeNavigationCallbacks = (_, callbackType, ctx) =>
                {
                    callbackTypes.Add(callbackType);
                    ctx.ShouldEqual(navigationContext);
                    return true;
                }
            });

            _callbackInvoker.IsSuspended.ShouldBeFalse();
            var actionToken = NavigationDispatcher.GetComponents<ISuspendableComponent<INavigationDispatcher>>().TrySuspend(NavigationDispatcher, this, null);
            _callbackInvoker.IsSuspended.ShouldBeTrue();
            NavigationDispatcher.OnNavigated(navigationContext);

            callbackTypes.Count.ShouldEqual(0);
            actionToken.Dispose();
            _callbackInvoker.IsSuspended.ShouldBeFalse();
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
        public void OnNavigationCanceledShouldInvokeCallbacks(bool close)
        {
            var navigationContext = GetNavigationContext(this, close ? NavigationMode.Close : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (_, callbackType, ctx, ct) =>
                {
                    callbackTypes.Add(callbackType);
                    ctx.ShouldEqual(navigationContext);
                    ct.ShouldEqual(DefaultCancellationToken);
                    return true;
                },
                TryInvokeExceptionNavigationCallbacks = (_, _, _, _) => throw new NotSupportedException(),
                TryInvokeNavigationCallbacks = (_, _, _) => throw new NotSupportedException()
            });

            _callbackInvoker.IsSuspended.ShouldBeFalse();
            var actionToken = NavigationDispatcher.GetComponents<ISuspendableComponent<INavigationDispatcher>>().TrySuspend(NavigationDispatcher, this, null);
            _callbackInvoker.IsSuspended.ShouldBeTrue();
            NavigationDispatcher.OnNavigationCanceled(navigationContext, DefaultCancellationToken);

            callbackTypes.Count.ShouldEqual(0);
            actionToken.Dispose();
            _callbackInvoker.IsSuspended.ShouldBeFalse();
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnNavigationFailedShouldInvokeCallbacks(bool close)
        {
            var exception = new Exception();
            var navigationContext = GetNavigationContext(this, close ? NavigationMode.Close : NavigationMode.New);
            var callbackTypes = new List<NavigationCallbackType>();
            NavigationDispatcher.AddComponent(new TestNavigationCallbackManagerComponent
            {
                TryInvokeCanceledNavigationCallbacks = (_, _, _, _) => throw new NotSupportedException(),
                TryInvokeExceptionNavigationCallbacks = (_, callbackType, ctx, ex) =>
                {
                    callbackTypes.Add(callbackType);
                    ctx.ShouldEqual(navigationContext);
                    ex.ShouldEqual(exception);
                    return true;
                },
                TryInvokeNavigationCallbacks = (_, _, _) => throw new NotSupportedException()
            });

            _callbackInvoker.IsSuspended.ShouldBeFalse();

            var actionToken = NavigationDispatcher.GetComponents<ISuspendableComponent<INavigationDispatcher>>().TrySuspend(NavigationDispatcher, this, null);
            _callbackInvoker.IsSuspended.ShouldBeTrue();
            NavigationDispatcher.OnNavigationFailed(navigationContext, exception);

            callbackTypes.Count.ShouldEqual(0);
            actionToken.Dispose();
            _callbackInvoker.IsSuspended.ShouldBeFalse();
            callbackTypes.Count.ShouldEqual(3);
            callbackTypes.ShouldContain(NavigationCallbackType.Close);
            callbackTypes.ShouldContain(NavigationCallbackType.Closing);
            callbackTypes.ShouldContain(NavigationCallbackType.Showing);
        }

        protected override INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);
    }
}