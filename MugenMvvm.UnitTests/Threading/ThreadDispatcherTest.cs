using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Threading
{
    public class ThreadDispatcherTest : ComponentOwnerTestBase<ThreadDispatcher>
    {
        [Fact]
        public void CanExecuteInlineShouldReturnFalseNoComponents() => GetComponentOwner(ComponentCollectionManager).CanExecuteInline(ThreadExecutionMode.MainAsync).ShouldBeFalse();

        [Fact]
        public void ExecuteShouldThrowNoComponents()
        {
            var dispatcher = GetComponentOwner(ComponentCollectionManager);
            ShouldThrow<InvalidOperationException>(() => dispatcher.Execute(ThreadExecutionMode.Background, t => { }, dispatcher));
            ShouldThrow<InvalidOperationException>(() => dispatcher.Execute(ThreadExecutionMode.Background, new TestThreadDispatcherHandler(), dispatcher));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanExecuteInlineShouldBeHandledByComponents(int count)
        {
            var dispatcher = GetComponentOwner(ComponentCollectionManager);
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var component = new TestThreadDispatcherComponent(dispatcher)
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

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ExecuteShouldBeHandledByComponents(int count)
        {
            var dispatcher = GetComponentOwner(ComponentCollectionManager);
            var executeCount = 0;
            var mode = ThreadExecutionMode.Background;
            object? handler = null;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestThreadDispatcherComponent(dispatcher)
                {
                    TryExecute = (m, h, state, meta) =>
                    {
                        ++executeCount;
                        h.ShouldEqual(handler);
                        mode.ShouldEqual(m);
                        state.ShouldEqual(dispatcher);
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
            handler = new TestThreadDispatcherHandler();
            dispatcher.TryExecute(mode, handler, dispatcher, DefaultMetadata);
            executeCount.ShouldEqual(count);
        }

        protected override ThreadDispatcher GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}