using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Commands;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CommandCleanerTest : UnitTestBase
    {
        public CommandCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void ShouldRegisterDisposeToken()
        {
            var cmd = new CompositeCommand();
            CommandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (_, _, _, _) => cmd
            });

            ActionToken? actionToken = null;
            var owner = new TestHasDisposeCallback
            {
                RegisterDisposeToken = token =>
                {
                    actionToken.ShouldBeNull();
                    actionToken = token;
                }
            };

            var command = CommandManager.GetCommand<object>(owner, this);
            command.ShouldEqual(cmd);
            cmd.IsDisposed.ShouldBeFalse();
            actionToken!.Value.Dispose();
            cmd.IsDisposed.ShouldBeTrue();
        }

        protected override ICommandManager GetCommandManager()
        {
            var commandManager = new CommandManager(ComponentCollectionManager);
            commandManager.AddComponent(new CommandCleaner());
            return commandManager;
        }
    }
}