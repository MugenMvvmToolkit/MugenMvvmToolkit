using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Threading
{
    public class ThreadDispatcherTest : ComponentOwnerTestBase<ThreadDispatcher>
    {
        #region Methods

        [Fact]
        public void CanExecuteInlineShouldReturnFalseNoComponents()
        {
            new ThreadDispatcher().CanExecuteInline(ThreadExecutionMode.MainAsync).ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanExecuteInlineShouldBeHandledByComponents(int count)
        {
            var dispatcher = new ThreadDispatcher();
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new TestThreadDispatcherComponent
                {
                    Priority = -i,
                    CanExecuteInline = (mode, m) =>
                    {
                        ++executeCount;
                        mode.ShouldEqual(mode);
                        m.ShouldEqual(DefaultMetadata);
                        return result;
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.CanExecuteInline(mode, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            mode = ThreadExecutionMode.Current;
            dispatcher.CanExecuteInline(mode, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void ExecuteShouldThrowNoComponents()
        {
            var dispatcher = new ThreadDispatcher();
            ShouldThrow<InvalidOperationException>(() => dispatcher.Execute(ThreadExecutionMode.Background, dispatcher, t => { }));
            ShouldThrow<InvalidOperationException>(() => dispatcher.Execute(ThreadExecutionMode.Background, new TestThreadDispatcherHandler<object>(), dispatcher));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ExecuteShouldBeHandledByComponents(int count)
        {
            var dispatcher = new ThreadDispatcher();
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            object? handler = null;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestThreadDispatcherComponent
                {
                    TryExecute = (m, h, state, stateType, meta) =>
                    {
                        ++executeCount;
                        h.ShouldEqual(handler);
                        mode.ShouldEqual(m);
                        state.ShouldEqual(dispatcher);
                        stateType.ShouldEqual(dispatcher.GetType());
                        meta.ShouldEqual(DefaultMetadata);
                        return isLast;
                    },
                    Priority = -i
                };
                dispatcher.AddComponent(component);
            }

            handler = new Action<ThreadDispatcher>(t => { });
            dispatcher.TryExecute(mode, handler, dispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            handler = new TestThreadDispatcherHandler<ThreadDispatcher>();
            dispatcher.TryExecute(mode, handler, dispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);
        }

        protected override ThreadDispatcher GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ThreadDispatcher(collectionProvider);
        }

        #endregion
    }
}