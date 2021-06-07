using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Commands
{
    public class TestCommandProviderComponent : ICommandProviderComponent
    {
        public Func<ICommandManager, object?, object, IReadOnlyMetadataContext?, ICompositeCommand?>? TryGetCommand { get; set; }

        ICompositeCommand? ICommandProviderComponent.TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata) =>
            TryGetCommand?.Invoke(commandManager, owner, request, metadata);
    }
}