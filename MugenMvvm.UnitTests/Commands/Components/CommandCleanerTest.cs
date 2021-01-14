using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Commands.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CommandCleanerTest : UnitTestBase
    {
        [Fact]
        public void ShouldRegisterDisposeToken()
        {
            var cmd = new CompositeCommand();
            var commandManager = new CommandManager();
            commandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (_, _, _) => cmd
            });
            commandManager.AddComponent(new CommandCleaner());

            ActionToken? actionToken = null;
            var owner = new TestHasDisposeCallback
            {
                RegisterDisposeToken = token =>
                {
                    actionToken.ShouldBeNull();
                    actionToken = token;
                }
            };

            var command = commandManager.GetCommand<object>(owner, this);
            command.ShouldEqual(cmd);
            cmd.IsDisposed.ShouldBeFalse();
            actionToken!.Value.Dispose();
            cmd.IsDisposed.ShouldBeTrue();
        }
    }
}