using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Components;
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
                    CanExecuteInline = mode =>
                    {
                        ++executeCount;
                        mode.ShouldEqual(mode);
                        return result;
                    }
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.CanExecuteInline(mode).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            mode = ThreadExecutionMode.Current;
            dispatcher.CanExecuteInline(mode).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void ExecuteShouldThrowNoComponents()
        {
            var dispatcher = new ThreadDispatcher();
            ShouldThrow<InvalidOperationException>(() => dispatcher.Execute(ThreadExecutionMode.Background, t => { }, dispatcher));
            ShouldThrow<InvalidOperationException>(() => dispatcher.Execute(ThreadExecutionMode.Background, new TestThreadDispatcherHandler<object>()));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ExecuteShouldBeHandledByComponents(int count)
        {
            var dispatcher = new ThreadDispatcher();
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestThreadDispatcherComponent
                {
                    Execute = (action, m, arg3, arg4, arg5) =>
                    {
                        ++executeCount;
                        mode.ShouldEqual(m);
                        arg3.ShouldEqual(dispatcher);
                        arg4.ShouldEqual(dispatcher.GetType());
                        arg5.ShouldEqual(DefaultMetadata);
                        return isLast;
                    },
                    Priority = -i
                };
                dispatcher.AddComponent(component);
            }

            dispatcher.Execute(mode, t => { }, dispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            dispatcher.Execute(mode, new TestThreadDispatcherHandler<ThreadDispatcher>(), dispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);
        }

        protected override ThreadDispatcher GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ThreadDispatcher(collectionProvider);
        }

        #endregion
    }
}