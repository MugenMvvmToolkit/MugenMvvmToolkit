using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestCommandProviderComponent : ICommandProviderComponent
    {
        #region Properties

        public Func<ICommandManager, object?, Type, IReadOnlyMetadataContext?, ICompositeCommand?>? TryGetCommand { get; set; }

        #endregion

        #region Implementation of interfaces

        ICompositeCommand? ICommandProviderComponent.TryGetCommand<TRequest>(ICommandManager commandManager, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetCommand?.Invoke(commandManager, request, typeof(TRequest), metadata);
        }

        #endregion
    }
}