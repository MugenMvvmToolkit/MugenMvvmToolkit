using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestCommandProviderListener : ICommandProviderListener
    {
        #region Properties

        public Action<ICommandProvider, object?, Type, ICompositeCommand, IReadOnlyMetadataContext?>? OnCommandCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICommandProviderListener.OnCommandCreated<TRequest>(ICommandProvider provider, ICompositeCommand command, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnCommandCreated?.Invoke(provider, request, typeof(TRequest), command, metadata);
        }

        #endregion
    }
}