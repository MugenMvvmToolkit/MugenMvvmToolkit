using System;
using MugenMvvm.Commands;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Commands.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Commands
{
    public class CommandManagerTest : ComponentOwnerTestBase<ICommandManager>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldBeHandledByComponents(int componentCount)
        {
            var commandManager = GetComponentOwner();
            ICompositeCommand command = new CompositeCommand();
            var count = 0;
            for (int i = 0; i < componentCount; i++)
            {
                var component = new TestCommandProviderComponent
                {
                    TryGetCommand = (o, type, arg3) =>
                    {
                        ++count;
                        o.ShouldEqual(commandManager);
                        type.ShouldEqual(typeof(ICommandManager));
                        arg3.ShouldEqual(DefaultMetadata);
                        return command;
                    }
                };
                commandManager.AddComponent(component);
            }

            var compositeCommand = commandManager.GetCommand(commandManager, DefaultMetadata);
            compositeCommand.ShouldEqual(command);
            count.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldNotifyListeners(int componentCount)
        {
            var commandManager = GetComponentOwner();
            ICompositeCommand command = new CompositeCommand();
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) => command
            };
            commandManager.AddComponent(component);

            var count = 0;
            for (int i = 0; i < componentCount; i++)
            {
                var listener = new TestCommandManagerListener
                {
                    OnCommandCreated = (provider, o, arg3, arg4, arg5) =>
                    {
                        provider.ShouldEqual(commandManager);
                        o.ShouldEqual(commandManager);
                        arg3.ShouldEqual(typeof(ICommandManager));
                        arg4.ShouldEqual(command);
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                };
                commandManager.AddComponent(listener);
            }

            commandManager.GetCommand(commandManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetCommandShouldThrowNoComponents()
        {
            var commandManager = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => commandManager.GetCommand(commandManager, DefaultMetadata));
        }

        protected override ICommandManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new CommandManager(collectionProvider);
        }

        #endregion
    }
}