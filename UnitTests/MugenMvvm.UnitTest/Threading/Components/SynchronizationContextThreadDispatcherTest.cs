using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Threading.Components;
using MugenMvvm.UnitTest.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Threading.Components
{
    public class SynchronizationContextThreadDispatcherTest : UnitTestBase
    {
        #region Fields

        private readonly TestSynchronizationContext _synchronizationContext;

        #endregion

        #region Constructors

        public SynchronizationContextThreadDispatcherTest()
        {
            _synchronizationContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }

        #endregion

        #region Methods

        [Fact]
        public void CanExecuteInlineShouldReturnTrueCurrent()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);
            component.CanExecuteInline(null!, ThreadExecutionMode.Current, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseBackground()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);
            component.CanExecuteInline(null!, ThreadExecutionMode.Background, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseMainAsync()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);
            component.CanExecuteInline(null!, ThreadExecutionMode.MainAsync, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnTrueMain()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);
            component.CanExecuteInline(null!, ThreadExecutionMode.Main, DefaultMetadata).ShouldBeTrue();
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
        public void TryExecuteShouldExecuteInline()
        {
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);

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
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);
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
            var component = new SynchronizationContextThreadDispatcher(_synchronizationContext, true);
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
            component.TryExecute(null!, ThreadExecutionMode.Background, action, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Background, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Background, handler, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Background, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(null!, ThreadExecutionMode.Background, callback, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            component.TryExecute(null!, ThreadExecutionMode.Background, component, component, DefaultMetadata).ShouldBeFalse();
        }

        private static void WaitThreadPool()
        {
            var taskCompletionSource = new TaskCompletionSource<object?>();
            ThreadPool.QueueUserWorkItem(state => taskCompletionSource.SetResult(null));
            taskCompletionSource.Task.Wait();
            Task.Delay(10).Wait();
        }

        #endregion

        #region Nested types

        private sealed class TestSynchronizationContext : SynchronizationContext
        {
            #region Properties

            public SendOrPostCallback? Callback { get; set; }

            public object? State { get; set; }

            #endregion

            #region Methods

            public void Invoke()
            {
                Callback?.Invoke(State);
                Callback = null;
                State = null;
            }

            public override void Post(SendOrPostCallback d, object? state)
            {
                Callback = d;
                State = state;
            }

            #endregion
        }

        #endregion
    }
}