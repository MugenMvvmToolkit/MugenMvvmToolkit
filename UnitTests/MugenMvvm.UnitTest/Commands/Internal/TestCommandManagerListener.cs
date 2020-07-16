using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestCommandManagerListener : ICommandManagerListener
    {
        #region Fields

        private readonly ICommandManager? _commandManager;

        #endregion

        #region Constructors

        public TestCommandManagerListener(ICommandManager? commandManager = null)
        {
            _commandManager = commandManager;
        }

        #endregion

        #region Properties

        public Action<object?, ICompositeCommand, IReadOnlyMetadataContext?>? OnCommandCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICommandManagerListener.OnCommandCreated<TParameter>(ICommandManager provider, ICompositeCommand command, object request, IReadOnlyMetadataContext? metadata)
        {
            _commandManager?.ShouldEqual(provider);
            OnCommandCreated?.Invoke(request, command, metadata);
        }

        #endregion
    }
}