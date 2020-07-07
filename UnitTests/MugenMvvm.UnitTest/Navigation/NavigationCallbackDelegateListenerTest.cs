﻿using System;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Internal;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationCallbackDelegateListenerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DisposeTargetCallbackShouldDisposeTarget()
        {
            var invokeCount = 0;
            var disposable = new TestDisposable
            {
                Dispose = () => ++invokeCount
            };
            var ctx = new NavigationContext(disposable, Default.NavigationProvider, "f", NavigationType.Alert, NavigationMode.Background);
            NavigationCallbackDelegateListener.DisposeTargetCallback.OnCompleted(ctx);
            invokeCount.ShouldEqual(1);

            NavigationCallbackDelegateListener.DisposeTargetCallback.OnError(ctx, new Exception());
            invokeCount.ShouldEqual(2);

            NavigationCallbackDelegateListener.DisposeTargetCallback.OnCanceled(ctx, default);
            invokeCount.ShouldEqual(3);
        }

        [Fact]
        public void OnCompletedShouldInvokeDelegate()
        {
            var ctx = new NavigationContext(this, Default.NavigationProvider, "f", NavigationType.Alert, NavigationMode.Background);
            Exception? error = null;
            CancellationToken? token = null;
            var invokeCount = 0;
            var listener = new NavigationCallbackDelegateListener((context, exception, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(ctx);
                exception.ShouldEqual(error);
                arg3.ShouldEqual(token);
            });
            listener.OnCompleted(ctx);
            invokeCount.ShouldEqual(invokeCount);
        }

        [Fact]
        public void OnErrorShouldInvokeDelegate()
        {
            var ctx = new NavigationContext(this, Default.NavigationProvider, "f", NavigationType.Alert, NavigationMode.Background);
            var error = new Exception();
            CancellationToken? token = null;
            var invokeCount = 0;
            var listener = new NavigationCallbackDelegateListener((context, exception, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(ctx);
                exception.ShouldEqual(error);
                arg3.ShouldEqual(token);
            });
            listener.OnError(ctx, error);
            invokeCount.ShouldEqual(invokeCount);
        }

        [Fact]
        public void OnCanceledShouldInvokeDelegate()
        {
            var ctx = new NavigationContext(this, Default.NavigationProvider, "f", NavigationType.Alert, NavigationMode.Background);
            Exception? error = null;
            var token = new CancellationToken(true);
            var invokeCount = 0;
            var listener = new NavigationCallbackDelegateListener((context, exception, arg3) =>
            {
                ++invokeCount;
                context.ShouldEqual(ctx);
                exception.ShouldEqual(error);
                arg3.ShouldEqual(token);
            });
            listener.OnCanceled(ctx, token);
            invokeCount.ShouldEqual(invokeCount);
        }

        #endregion
    }
}