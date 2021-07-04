using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandCleaner : ICommandManagerListener, IHasPriority
    {
        public int Priority { get; init; } = CommandComponentPriority.CommandCleaner;

        public void OnCommandCreated<TParameter>(ICommandManager commandManager, ICompositeCommand command, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IHasDisposeCallback hasDisposeCallback)
                hasDisposeCallback.RegisterDisposeToken(ActionToken.FromDisposable(command));
        }
    }
}