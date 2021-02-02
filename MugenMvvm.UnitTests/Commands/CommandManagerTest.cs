using System;
using MugenMvvm.Commands;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Commands.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Commands
{
    public class CommandManagerTest : ComponentOwnerTestBase<ICommandManager>
    {
        [Fact]
        public void GetCommandShouldThrowNoComponents()
        {
            var commandManager = GetComponentOwner(ComponentCollectionManager);
            ShouldThrow<InvalidOperationException>(() => commandManager.GetCommand<string>(commandManager, commandManager, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldBeHandledByComponents(int componentCount)
        {
            var owner = new object();
            var commandManager = GetComponentOwner(ComponentCollectionManager);
            ICompositeCommand command = new CompositeCommand();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandProviderComponent(commandManager)
                {
                    TryGetCommand = (o, r, m) =>
                    {
                        ++count;
                        o.ShouldEqual(owner);
                        r.ShouldEqual(commandManager);
                        m.ShouldEqual(DefaultMetadata);
                        return command;
                    }
                };
                commandManager.AddComponent(component);
            }

            var compositeCommand = commandManager.GetCommand<int>(owner, commandManager, DefaultMetadata);
            compositeCommand.ShouldEqual(command);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldNotifyListeners(int componentCount)
        {
            var owner = new object();
            var commandManager = GetComponentOwner(ComponentCollectionManager);
            ICompositeCommand command = new CompositeCommand();
            var component = new TestCommandProviderComponent(commandManager)
            {
                TryGetCommand = (_, _, _) => command
            };
            commandManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestCommandManagerListener(commandManager)
                {
                    OnCommandCreated = (o, r, c, m) =>
                    {
                        o.ShouldEqual(owner);
                        r.ShouldEqual(commandManager);
                        c.ShouldEqual(command);
                        m.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                };
                commandManager.AddComponent(listener);
            }

            commandManager.GetCommand<string>(owner, commandManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        protected override ICommandManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new CommandManager(componentCollectionManager);
    }
}