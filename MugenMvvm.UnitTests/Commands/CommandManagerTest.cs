using System;
using MugenMvvm.Commands;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Commands;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands
{
    public class CommandManagerTest : ComponentOwnerTestBase<ICommandManager>
    {
        [Fact]
        public void GetCommandShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() => CommandManager.GetCommand<string>(CommandManager, CommandManager, DefaultMetadata));

        protected override ICommandManager GetCommandManager() => GetComponentOwner(ComponentCollectionManager);

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldBeHandledByComponents(int componentCount)
        {
            var owner = new object();
            ICompositeCommand command = new CompositeCommand();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                CommandManager.AddComponent(new TestCommandProviderComponent
                {
                    TryGetCommand = (c, o, r, m) =>
                    {
                        ++count;
                        c.ShouldEqual(CommandManager);
                        o.ShouldEqual(owner);
                        r.ShouldEqual(CommandManager);
                        m.ShouldEqual(DefaultMetadata);
                        return command;
                    }
                });
            }

            var compositeCommand = CommandManager.GetCommand<int>(owner, CommandManager, DefaultMetadata);
            compositeCommand.ShouldEqual(command);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldNotifyListeners(int componentCount)
        {
            var owner = new object();
            CommandManager.AddComponent(new TestCommandProviderComponent
            {
                TryGetCommand = (_, _, _, _) => Command
            });

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                CommandManager.AddComponent(new TestCommandManagerListener
                {
                    OnCommandCreated = (manager, o, r, c, m) =>
                    {
                        manager.ShouldEqual(CommandManager);
                        CommandManager.ShouldEqual(manager);
                        r.ShouldEqual(CommandManager);
                        c.ShouldEqual(Command);
                        m.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                });
            }

            CommandManager.GetCommand<string>(owner, CommandManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        protected override ICommandManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new CommandManager(componentCollectionManager);
    }
}