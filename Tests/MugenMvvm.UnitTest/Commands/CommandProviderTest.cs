using System;
using MugenMvvm.Commands;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Commands
{
    public class CommandProviderTest : ComponentOwnerTestBase<ICommandProvider>
    {
        #region Methods

        [Fact]
        public void GetCommandShouldBeHandledByComponents()
        {
            var commandProvider = GetComponentOwner();
            ICompositeCommand command = new CompositeCommand();
            var count = 0;
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) =>
                {
                    ++count;
                    o.ShouldEqual(commandProvider);
                    type.ShouldEqual(typeof(ICommandProvider));
                    arg3.ShouldEqual(DefaultMetadata);
                    return command;
                }
            };
            commandProvider.AddComponent(component);

            var compositeCommand = commandProvider.GetCommand(commandProvider, DefaultMetadata);
            compositeCommand.ShouldEqual(command);
            count.ShouldEqual(1);
        }

        [Fact]
        public void GetCommandShouldNotifyListeners()
        {
            var commandProvider = GetComponentOwner();
            ICompositeCommand command = new CompositeCommand();
            var component = new TestCommandProviderComponent
            {
                TryGetCommand = (o, type, arg3) => command
            };
            commandProvider.AddComponent(component);

            var count = 0;
            var listener = new TestCommandProviderListener
            {
                OnCommandCreated = (provider, o, arg3, arg4, arg5) =>
                {
                    provider.ShouldEqual(commandProvider);
                    o.ShouldEqual(commandProvider);
                    arg3.ShouldEqual(typeof(ICommandProvider));
                    arg4.ShouldEqual(command);
                    arg5.ShouldEqual(DefaultMetadata);
                    ++count;
                }
            };
            commandProvider.AddComponent(listener);

            commandProvider.GetCommand(commandProvider, DefaultMetadata);
            count.ShouldEqual(1);
        }

        [Fact]
        public void GetCommandShouldThrowNoComponents()
        {
            var commandProvider = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => commandProvider.GetCommand(commandProvider, DefaultMetadata));
        }

        protected override ICommandProvider GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new CommandProvider(collectionProvider);
        }

        #endregion
    }
}