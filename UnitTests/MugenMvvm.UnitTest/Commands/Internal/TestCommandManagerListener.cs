using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestCommandManagerListener : ICommandManagerListener
    {
        #region Properties

        public Action<ICommandManager, object?, Type, ICompositeCommand, IReadOnlyMetadataContext?>? OnCommandCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICommandManagerListener.OnCommandCreated<TRequest>(ICommandManager provider, ICompositeCommand command, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnCommandCreated?.Invoke(provider, request, typeof(TRequest), command, metadata);
        }

        #endregion
    }
}