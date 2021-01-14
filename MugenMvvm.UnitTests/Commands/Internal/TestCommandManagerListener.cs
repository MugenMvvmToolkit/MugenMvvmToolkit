using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestCommandManagerListener : ICommandManagerListener
    {
        private readonly ICommandManager? _commandManager;

        public TestCommandManagerListener(ICommandManager? commandManager = null)
        {
            _commandManager = commandManager;
        }

        public Action<object?, object, ICompositeCommand, IReadOnlyMetadataContext?>? OnCommandCreated { get; set; }

        void ICommandManagerListener.OnCommandCreated<TParameter>(ICommandManager commandManager, ICompositeCommand command, object? owner, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _commandManager?.ShouldEqual(commandManager);
            OnCommandCreated?.Invoke(owner, request, command, metadata);
        }
    }
}