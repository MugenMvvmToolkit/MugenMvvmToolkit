using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Threading
{
    public class ThreadDispatcherTest : ComponentOwnerTestBase<ThreadDispatcher>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanExecuteInlineShouldBeHandledByComponents(int count)
        {
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new TestThreadDispatcherComponent
                {
                    Priority = -i,
                    CanExecuteInline = (d, mode, m) =>
                    {
                        d.ShouldEqual(ThreadDispatcher);
                        mode.ShouldEqual(mode);
                        m.ShouldEqual(DefaultMetadata);
                        ++executeCount;
                        return result;
                    }
                };
                ThreadDispatcher.AddComponent(component);
            }

            ThreadDispatcher.CanExecuteInline(mode, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            result = true;
            mode = ThreadExecutionMode.Current;
            ThreadDispatcher.CanExecuteInline(mode, DefaultMetadata).ShouldEqual(result);
            executeCount.ShouldEqual(1);
        }

        [Fact]
        public void CanExecuteInlineShouldReturnFalseNoComponents() =>
            ThreadDispatcher.CanExecuteInline(ThreadExecutionMode.MainAsync).ShouldBeFalse();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ExecuteShouldBeHandledByComponents(int count)
        {
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            object? handler = null;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
                {
                    TryExecute = (d, m, h, state, meta) =>
                    {
                        d.ShouldEqual(ThreadDispatcher);
                        h.ShouldEqual(handler);
                        mode.ShouldEqual(m);
                        state.ShouldEqual(ThreadDispatcher);
                        meta.ShouldEqual(DefaultMetadata);
                        ++executeCount;
                        return isLast;
                    },
                    Priority = -i
                });
            }

            handler = new Action<ThreadDispatcher>(t => { });
            ThreadDispatcher.TryExecute(mode, handler, ThreadDispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);

            executeCount = 0;
            handler = new TestThreadDispatcherHandler();
            ThreadDispatcher.TryExecute(mode, handler, ThreadDispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);
        }

        [Fact]
        public void ExecuteShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => ThreadDispatcher.Execute(ThreadExecutionMode.Background, t => { }, ThreadDispatcher));
            ShouldThrow<InvalidOperationException>(() => ThreadDispatcher.Execute(ThreadExecutionMode.Background, new TestThreadDispatcherHandler(), ThreadDispatcher));
        }

        protected override IThreadDispatcher GetThreadDispatcher() => GetComponentOwner(ComponentCollectionManager);

        protected override ThreadDispatcher GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}