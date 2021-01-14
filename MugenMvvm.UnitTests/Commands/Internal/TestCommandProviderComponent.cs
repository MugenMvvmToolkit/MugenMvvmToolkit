using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestCommandProviderComponent : ICommandProviderComponent
    {
        private readonly ICommandManager? _commandManager;

        public TestCommandProviderComponent(ICommandManager? commandManager = null)
        {
            _commandManager = commandManager;
        }

        public Func<object?, object, IReadOnlyMetadataContext?, ICompositeCommand?>? TryGetCommand { get; set; }

        ICompositeCommand? ICommandProviderComponent.TryGetCommand<TParameter>(ICommandManager commandManager, object? owner, object request, IReadOnlyMetadataContext? metadata)
        {
            _commandManager?.ShouldEqual(commandManager);
            return TryGetCommand?.Invoke(owner, request, metadata);
        }
    }
}