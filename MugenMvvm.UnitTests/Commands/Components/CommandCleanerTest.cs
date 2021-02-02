using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Commands.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Commands.Components
{
    public class CommandCleanerTest : UnitTestBase
    {
        private readonly CommandManager _commandManager;

        public CommandCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _commandManager = new CommandManager(ComponentCollectionManager);
            _commandManager.AddComponent(new CommandCleaner());
        }

        [Fact]
        public void ShouldRegisterDisposeToken()
        {
            var cmd = new CompositeCommand();
            _commandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (_, _, _) => cmd
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

            var command = _commandManager.GetCommand<object>(owner, this);
            command.ShouldEqual(cmd);
            cmd.IsDisposed.ShouldBeFalse();
            actionToken!.Value.Dispose();
            cmd.IsDisposed.ShouldBeTrue();
        }
    }
}