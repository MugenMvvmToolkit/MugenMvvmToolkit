using System;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CommandEventHandlerTest : UnitTestBase
    {
        public CommandEventHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Command.AddComponent(new CommandEventHandler());
        }

        [Fact]
        public void DisposeShouldClearEventHandler()
        {
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
            var executed = 0;
            EventHandler handler = (sender, args) =>
            {
                sender.ShouldEqual(Command);
                ++executed;
            };
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            Command.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            Command.CanExecuteChanged -= handler;
            Command.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);
        }

        [Fact]
        public void ShouldSuspendNotifications()
        {
            var executed = 0;
            EventHandler handler = (sender, args) => ++executed;
            Command.CanExecuteChanged += handler;

            executed.ShouldEqual(0);
            Command.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            var actionToken1 = Command.Suspend();
            var actionToken2 = Command.Suspend();
            Command.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            actionToken1.Dispose();
            Command.RaiseCanExecuteChanged();
            executed.ShouldEqual(1);

            actionToken2.Dispose();
            executed.ShouldEqual(2);
        }
    }
}