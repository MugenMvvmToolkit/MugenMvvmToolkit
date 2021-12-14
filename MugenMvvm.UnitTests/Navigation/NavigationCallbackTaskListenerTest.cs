using System;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Navigation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation
{
    public class NavigationCallbackTaskListenerTest : UnitTestBase
    {
        [Fact]
        public void OnCanceledCancellationTokenShouldCompleteTask()
        {
            var cts = new CancellationTokenSource();
            var listener = new NavigationCallbackTaskListener(false, cts.Token);
            listener.Task.IsCompleted.ShouldBeFalse();
            cts.Cancel();
            listener.Task.IsCompleted.ShouldBeTrue();
            listener.Task.IsFaulted.ShouldBeFalse();
            listener.Task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void OnCanceledShouldCompleteTask()
        {
            var ctx = new NavigationContext(this, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            var listener = new NavigationCallbackTaskListener(false, default);
            listener.Task.IsCompleted.ShouldBeFalse();
            listener.OnCanceled(ctx, default);
            listener.Task.IsCompleted.ShouldBeTrue();
            listener.Task.IsFaulted.ShouldBeFalse();
            listener.Task.IsCanceled.ShouldBeTrue();
        }

        [Fact]
        public void OnCompletedShouldCompleteTask()
        {
            var ctx = new NavigationContext(this, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            var listener = new NavigationCallbackTaskListener(true, default);
            listener.IsSerializable.ShouldBeTrue();
            listener.Task.IsCompleted.ShouldBeFalse();
            listener.OnCompleted(ctx);
            listener.Task.IsCompleted.ShouldBeTrue();
            listener.Task.IsFaulted.ShouldBeFalse();
            listener.Task.IsCanceled.ShouldBeFalse();
        }

        [Fact]
        public void OnErrorShouldCompleteTask()
        {
            var ctx = new NavigationContext(this, NavigationProvider.System, "f", NavigationType.Alert, NavigationMode.New);
            var error = new Exception();
            var listener = new NavigationCallbackTaskListener(false, default);
            listener.IsSerializable.ShouldBeFalse();
            listener.Task.IsCompleted.ShouldBeFalse();
            listener.OnError(ctx, error);
            listener.Task.IsCompleted.ShouldBeTrue();
            listener.Task.IsFaulted.ShouldBeTrue();
            listener.Task.IsCanceled.ShouldBeFalse();
        }
    }
}