using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using MugenMvvm.Threading.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Threading.Components
{
    public class SynchronizationContextThreadDispatcherTest : UnitTestBase
    {
        private readonly TestSynchronizationContext _synchronizationContext;

        public SynchronizationContextThreadDispatcherTest()
        {
            _synchronizationContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
            ThreadDispatcher.AddComponent(new SynchronizationContextThreadDispatcher(_synchronizationContext));
        }

        [Fact]
        public void CanExecuteInlineShouldMainWaitPost()
        {
            _synchronizationContext.Callback.ShouldBeNull();
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext);
            component.CanExecuteInline(null!, ThreadExecutionMode.Main, DefaultMetadata).ShouldBeFalse();

            _synchronizationContext.Callback.ShouldNotBeNull();
            _synchronizationContext.Invoke();
            component.CanExecuteInline(null!, ThreadExecutionMode.Main, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseBackground()
        {
            var component = new SynchronizationContextThreadDispatcher(SynchronizationContext.Current!);
            component.CanExecuteInline(null!, ThreadExecutionMode.Background, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseMainAsync() => ThreadDispatcher.CanExecuteInline(ThreadExecutionMode.MainAsync, DefaultMetadata).ShouldBeFalse();

        [Fact]
        public void CanExecuteInlineShouldReturnTrueCurrent() => ThreadDispatcher.CanExecuteInline(ThreadExecutionMode.Current, DefaultMetadata).ShouldBeTrue();

        [Fact]
        public void CanExecuteInlineShouldReturnTrueMain()
        {
            var component = new SynchronizationContextThreadDispatcher(SynchronizationContext.Current!);
            component.CanExecuteInline(null!, ThreadExecutionMode.Main, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void TryExecuteShouldExecuteInline()
        {
            var executed = 0;
            var state = new object();
            Action action = () => ++executed;
            Action<object> actionWithState = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };
            var handler = new TestThreadDispatcherHandler
            {
                Execute = _ => ++executed
            };
            var handlerWithState = new TestThreadDispatcherHandler
            {
                Execute = o =>
                {
                    ++executed;
                    o.ShouldEqual(state);
                }
            };

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.Current, action, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.Current, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.Current, handler, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.Current, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            ThreadDispatcher.TryExecute(ThreadExecutionMode.Current, ThreadDispatcher, ThreadDispatcher, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void TryExecuteShouldExecuteUsingContext()
        {
            var executed = 0;
            var state = new object();
            Action action = () => ++executed;
            Action<object> actionWithState = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };
            var handler = new TestThreadDispatcherHandler
            {
                Execute = _ => ++executed
            };
            var handlerWithState = new TestThreadDispatcherHandler
            {
                Execute = o =>
                {
                    ++executed;
                    o.ShouldEqual(state);
                }
            };
            SendOrPostCallback callback = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.MainAsync, action, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.MainAsync, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.MainAsync, handler, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.MainAsync, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.MainAsync, callback, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);


            ThreadDispatcher.TryExecute(ThreadExecutionMode.MainAsync, ThreadDispatcher, ThreadDispatcher, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void TryExecuteShouldExecuteUsingThreadPool()
        {
            var executed = 0;
            var state = new object();
            var tcs = new TaskCompletionSource<object?>();
            Action action = () =>
            {
                ++executed;
                tcs.SetResult(null);
            };
            Action<object> actionWithState = o =>
            {
                o.ShouldEqual(state);
                ++executed;
                tcs.SetResult(null);
            };
            var handler = new TestThreadDispatcherHandler
            {
                Execute = _ =>
                {
                    ++executed;
                    tcs.SetResult(null);
                }
            };

            var handlerWithState = new TestThreadDispatcherHandler
            {
                Execute = o =>
                {
                    ++executed;
                    o.ShouldEqual(state);
                    tcs.SetResult(null);
                }
            };
            WaitCallback callback = o =>
            {
                o.ShouldEqual(state);
                ++executed;
                tcs.SetResult(null);
            };

            executed = 0;
            ThreadDispatcher.TryExecute(ThreadExecutionMode.BackgroundAsync, action, state, DefaultMetadata).ShouldBeTrue();
            tcs.Task.Wait();
            executed.ShouldEqual(1);

            executed = 0;
            tcs = new TaskCompletionSource<object?>();
            ThreadDispatcher.TryExecute(ThreadExecutionMode.BackgroundAsync, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            tcs.Task.Wait();
            executed.ShouldEqual(1);

            executed = 0;
            tcs = new TaskCompletionSource<object?>();
            ThreadDispatcher.TryExecute(ThreadExecutionMode.BackgroundAsync, handler, state, DefaultMetadata).ShouldBeTrue();
            tcs.Task.Wait();
            executed.ShouldEqual(1);

            executed = 0;
            tcs = new TaskCompletionSource<object?>();
            ThreadDispatcher.TryExecute(ThreadExecutionMode.BackgroundAsync, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            tcs.Task.Wait();
            executed.ShouldEqual(1);

            executed = 0;
            tcs = new TaskCompletionSource<object?>();
            ThreadDispatcher.TryExecute(ThreadExecutionMode.BackgroundAsync, callback, state, DefaultMetadata).ShouldBeTrue();
            tcs.Task.Wait();
            executed.ShouldEqual(1);

            ThreadDispatcher.TryExecute(ThreadExecutionMode.BackgroundAsync, ThreadDispatcher, ThreadDispatcher, DefaultMetadata).ShouldBeFalse();
        }

        protected override IThreadDispatcher GetThreadDispatcher() => new ThreadDispatcher(ComponentCollectionManager);

        private sealed class TestSynchronizationContext : SynchronizationContext
        {
            public SendOrPostCallback? Callback { get; set; }

            public object? State { get; set; }

            public override void Post(SendOrPostCallback d, object? state)
            {
                Callback = d;
                State = state;
            }

            public void Invoke()
            {
                Callback?.Invoke(State);
                Callback = null;
                State = null;
            }
        }
    }
}