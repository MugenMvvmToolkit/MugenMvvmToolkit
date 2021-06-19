using System;
using MugenMvvm.Commands.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CommandEventHandlerTest : UnitTestBase
    {
        public CommandEventHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void DisposeShouldClearEventHandler()
        {
            var commandEventHandler = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(commandEventHandler);
            var executed = 0;
            EventHandler handler = (_, _) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            Command.Dispose();
            Command.RaiseCanExecuteChanged();
            executed.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSubscribeUnsubscribeRaiseEventHandler()
        {
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) =>
            {
                sender.ShouldEqual(Command);
                ++executed;
            };
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            Command.CanExecuteChanged -= handler;
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSuspendNotifications()
        {
            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, ThreadExecutionMode.Current);
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            var actionToken1 = conditionEventCommandComponent.Suspend(this, DefaultMetadata);
            var actionToken2 = conditionEventCommandComponent.Suspend(this, DefaultMetadata);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            actionToken1.Dispose();
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            actionToken2.Dispose();
            executed.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldUseLocalThreadDispatcher(int mode)
        {
            var executionMode = ThreadExecutionMode.Get(mode);
            Action? invoke = null;
            ThreadDispatcher.ClearComponents();
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                CanExecuteInline = (_, _, _) => false,
                Execute = (_, action, mode, arg3, _) =>
                {
                    mode.ShouldEqual(executionMode);
                    invoke = () => action(arg3);
                    return true;
                }
            });

            var conditionEventCommandComponent = new CommandEventHandler(ThreadDispatcher, executionMode);
            Command.AddComponent(conditionEventCommandComponent);
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            conditionEventCommandComponent.RaiseCanExecuteChanged();
            executed.ShouldEqual(0);
            invoke.ShouldNotBeNull();

            invoke!();
            executed.ShouldEqual(1);
        }
    }
}