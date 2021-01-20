using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Threading.Components;
using MugenMvvm.UnitTests.Threading.Internal;
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
        public void CanExecuteInlineShouldReturnFalseMainAsync()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext);
            component.CanExecuteInline(null!, ThreadExecutionMode.MainAsync, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnTrueCurrent()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext);
            component.CanExecuteInline(null!, ThreadExecutionMode.Current, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnTrueMain()
        {
            var component = new SynchronizationContextThreadDispatcher(SynchronizationContext.Current!);
            component.CanExecuteInline(null!, ThreadExecutionMode.Main, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void TryExecuteShouldExecuteInline()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext);

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
            component.TryExecute(null!, ThreadExecutionMode.Current, action, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Current, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Current, handler, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Current, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            component.TryExecute(null!, ThreadExecutionMode.Current, component, component, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void TryExecuteShouldExecuteUsingContext()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext);
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
            component.TryExecute(null!, ThreadExecutionMode.MainAsync, action, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.MainAsync, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.MainAsync, handler, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.MainAsync, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.MainAsync, callback, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);


            component.TryExecute(null!, ThreadExecutionMode.MainAsync, component, component, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void TryExecuteShouldExecuteUsingThreadPool()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext);
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
            WaitCallback callback = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.BackgroundAsync, action, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.BackgroundAsync, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.BackgroundAsync, handler, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.BackgroundAsync, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.BackgroundAsync, callback, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            component.TryExecute(null!, ThreadExecutionMode.BackgroundAsync, component, component, DefaultMetadata).ShouldBeFalse();
        }

        private static void WaitThreadPool()
        {
            var taskCompletionSource = new TaskCompletionSource<object?>();
            ThreadPool.QueueUserWorkItem(state => taskCompletionSource.SetResult(null));
            taskCompletionSource.Task.Wait();
            WaitCompletion();
        }

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