using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Commands
{
    public class TestCommandManagerListener : ICommandManagerListener
    {
        public Action<ICommandManager, object?, object, ICompositeCommand, IReadOnlyMetadataContext?>? OnCommandCreated { get; set; }

        void ICommandManagerListener.OnCommandCreated<TParameter>(ICommandManager commandManager, ICompositeCommand command, object? owner, object request,
            IReadOnlyMetadataContext? metadata) =>
            OnCommandCreated?.Invoke(commandManager, owner, request, command, metadata);
    }
}