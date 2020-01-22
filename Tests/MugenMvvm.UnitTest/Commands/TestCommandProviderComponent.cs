using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Commands
{
    public class TestCommandProviderComponent : ICommandProviderComponent
    {
        #region Properties

        public Func<object?, Type, IReadOnlyMetadataContext?, ICompositeCommand?>? TryGetCommand { get; set; }

        #endregion

        #region Implementation of interfaces

        ICompositeCommand? ICommandProviderComponent.TryGetCommand<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetCommand?.Invoke(request, typeof(TRequest), metadata);
        }

        #endregion
    }
}