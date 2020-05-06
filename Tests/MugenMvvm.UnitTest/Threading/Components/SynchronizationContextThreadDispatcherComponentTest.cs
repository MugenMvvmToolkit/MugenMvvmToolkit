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
    public class SynchronizationContextThreadDispatcherComponentTest : UnitTestBase
    {
        #region Fields

        private readonly TestSynchronizationContext _synchronizationContext;

        #endregion

        #region Constructors

        public SynchronizationContextThreadDispatcherComponentTest()
        {
            _synchronizationContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }

        #endregion

        #region Methods

        [Fact]
        public void CanExecuteInlineShouldReturnTrueCurrent()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);
            component.CanExecuteInline(ThreadExecutionMode.Current, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseBackground()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);
            component.CanExecuteInline(ThreadExecutionMode.Background, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseMainAsync()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);
            component.CanExecuteInline(ThreadExecutionMode.MainAsync, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void CanExecuteInlineShouldReturnTrueMain()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);
            component.CanExecuteInline(ThreadExecutionMode.Main, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void CanExecuteInlineShouldMainWaitPost()
        {
            _synchronizationContext.Callback.ShouldBeNull();
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, false);
            component.CanExecuteInline(ThreadExecutionMode.Main, DefaultMetadata).ShouldBeFalse();

            _synchronizationContext.Callback.ShouldNotBeNull();
            _synchronizationContext.Invoke();
            component.CanExecuteInline(ThreadExecutionMode.Main, DefaultMetadata).ShouldBeTrue();
        }

        [Fact]
        public void TryExecuteShouldExecuteInline()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);

            int executed = 0;
            var state = new object();
            Action action = () => ++executed;
            Action<object> actionWithState = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };
            var handler = new TestThreadDispatcherHandler
            {
                Execute = () => ++executed
            };
            var handlerWithState = new TestThreadDispatcherHandler<object?>
            {
                Execute = (o, type) =>
                {
                    ++executed;
                    o.ShouldEqual(state);
                    type.ShouldEqual(state.GetType());
                }
            };

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Current, action, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Current, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Current, handler, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Current, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(1);

            component.TryExecute(ThreadExecutionMode.Current, component, component, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void TryExecuteShouldExecuteUsingContext()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);
            int executed = 0;
            var state = new object();
            Action action = () => ++executed;
            Action<object> actionWithState = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };
            var handler = new TestThreadDispatcherHandler
            {
                Execute = () => ++executed
            };
            var handlerWithState = new TestThreadDispatcherHandler<object?>
            {
                Execute = (o, type) =>
                {
                    ++executed;
                    o.ShouldEqual(state);
                    type.ShouldEqual(state.GetType());
                }
            };
            SendOrPostCallback callback = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };

            executed = 0;
            component.TryExecute(ThreadExecutionMode.MainAsync, action, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.MainAsync, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.MainAsync, handler, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.MainAsync, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.MainAsync, callback, state, DefaultMetadata).ShouldBeTrue();
            executed.ShouldEqual(0);
            _synchronizationContext.Invoke();
            executed.ShouldEqual(1);


            component.TryExecute(ThreadExecutionMode.MainAsync, component, component, DefaultMetadata).ShouldBeFalse();
        }

        [Fact]
        public void TryExecuteShouldExecuteUsingThreadPool()
        {
            var component = new SynchronizationContextThreadDispatcherComponent(_synchronizationContext, true);
            int executed = 0;
            var state = new object();
            Action action = () => ++executed;
            Action<object> actionWithState = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };
            var handler = new TestThreadDispatcherHandler
            {
                Execute = () => ++executed
            };
            var handlerWithState = new TestThreadDispatcherHandler<object?>
            {
                Execute = (o, type) =>
                {
                    ++executed;
                    o.ShouldEqual(state);
                    type.ShouldEqual(state.GetType());
                }
            };
            WaitCallback callback = o =>
            {
                o.ShouldEqual(state);
                ++executed;
            };

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Background, action, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Background, actionWithState, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Background, handler, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Background, handlerWithState, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            executed = 0;
            component.TryExecute(ThreadExecutionMode.Background, callback, state, DefaultMetadata).ShouldBeTrue();
            WaitThreadPool();
            executed.ShouldEqual(1);

            component.TryExecute(ThreadExecutionMode.Background, component, component, DefaultMetadata).ShouldBeFalse();
        }

        private void WaitThreadPool()
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

            public override void Post(SendOrPostCallback d, object state)
            {
                Callback = d;
                State = state;
            }

            #endregion
        }

        #endregion
    }
}