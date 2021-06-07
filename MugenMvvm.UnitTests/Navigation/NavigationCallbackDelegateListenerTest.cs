﻿using System;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Navigation;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation
{
    public class NavigationCallbackDelegateListenerTest : UnitTestBase
    {
        [Fact]
        public void DisposeTargetCallbackShouldDisposeTarget()
        {
            var invokeCount = 0;
            var disposable = new TestDisposable
            {
                Dispose = () => ++invokeCount
            };
            var ctx = new NavigationContext(disposable, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            NavigationCallbackDelegateListener.DisposeTargetCallback.OnCompleted(ctx);
            invokeCount.ShouldEqual(1);

            NavigationCallbackDelegateListener.DisposeTargetCallback.OnError(ctx, new Exception());
            invokeCount.ShouldEqual(2);

            NavigationCallbackDelegateListener.DisposeTargetCallback.OnCanceled(ctx, default);
            invokeCount.ShouldEqual(3);
        }

        [Fact]
        public void OnCanceledShouldInvokeDelegate()
        {
            var ctx = new NavigationContext(this, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            Exception? error = null;
            var token = new CancellationToken(true);
            var invokeCount = 0;
            var listener = new NavigationCallbackDelegateListener((context, exception, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(ctx);
                exception.ShouldEqual(error);
                arg3.ShouldEqual(token);
            }, true);
            listener.OnCanceled(ctx, token);
            invokeCount.ShouldEqual(invokeCount);
        }

        [Fact]
        public void OnCompletedShouldInvokeDelegate()
        {
            var ctx = new NavigationContext(this, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            Exception? error = null;
            CancellationToken? token = null;
            var invokeCount = 0;
            var listener = new NavigationCallbackDelegateListener((context, exception, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(ctx);
                exception.ShouldEqual(error);
                arg3.ShouldEqual(token);
            }, true);
            listener.IsSerializable.ShouldBeTrue();
            listener.OnCompleted(ctx);
            invokeCount.ShouldEqual(invokeCount);
        }

        [Fact]
        public void OnErrorShouldInvokeDelegate()
        {
            var ctx = new NavigationContext(this, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            var error = new Exception();
            CancellationToken? token = null;
            var invokeCount = 0;
            var listener = new NavigationCallbackDelegateListener((context, exception, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(ctx);
                exception.ShouldEqual(error);
                arg3.ShouldEqual(token);
            }, true);
            listener.OnError(ctx, error);
            invokeCount.ShouldEqual(invokeCount);
        }
    }
}