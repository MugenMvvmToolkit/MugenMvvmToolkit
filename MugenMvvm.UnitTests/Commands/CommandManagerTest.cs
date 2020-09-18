﻿using System;
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
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetCommandShouldBeHandledByComponents(int componentCount)
        {
            var commandManager = GetComponentOwner();
            ICompositeCommand command = new CompositeCommand();
            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var component = new TestCommandProviderComponent(commandManager)
                {
                    TryGetCommand = (o, arg3) =>
                    {
                        ++count;
                        o.ShouldEqual(commandManager);
                        arg3.ShouldEqual(DefaultMetadata);
                        return command;
                    }
                };
                commandManager.AddComponent(component);
            }

            var compositeCommand = commandManager.GetCommand<int>(commandManager, DefaultMetadata);
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
            var component = new TestCommandProviderComponent(commandManager)
            {
                TryGetCommand = (o, arg3) => command
            };
            commandManager.AddComponent(component);

            var count = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var listener = new TestCommandManagerListener(commandManager)
                {
                    OnCommandCreated = (o, arg4, arg5) =>
                    {
                        o.ShouldEqual(commandManager);
                        arg4.ShouldEqual(command);
                        arg5.ShouldEqual(DefaultMetadata);
                        ++count;
                    }
                };
                commandManager.AddComponent(listener);
            }

            commandManager.GetCommand<string>(commandManager, DefaultMetadata);
            count.ShouldEqual(componentCount);
        }

        [Fact]
        public void GetCommandShouldThrowNoComponents()
        {
            var commandManager = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => commandManager.GetCommand<string>(commandManager, DefaultMetadata));
        }

        protected override ICommandManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new CommandManager(collectionProvider);

        #endregion
    }
}