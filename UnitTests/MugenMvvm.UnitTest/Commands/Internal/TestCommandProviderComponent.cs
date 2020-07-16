using System;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using Should;

namespace MugenMvvm.UnitTest.Commands.Internal
{
    public class TestCommandProviderComponent : ICommandProviderComponent
    {
        #region Fields

        private readonly ICommandManager? _commandManager;

        #endregion

        #region Constructors

        public TestCommandProviderComponent(ICommandManager? commandManager = null)
        {
            _commandManager = commandManager;
        }

        #endregion

        #region Properties

        public Func<object?, IReadOnlyMetadataContext?, ICompositeCommand?>? TryGetCommand { get; set; }

        #endregion

        #region Implementation of interfaces

        ICompositeCommand? ICommandProviderComponent.TryGetCommand<TParameter>(ICommandManager commandManager, object request, IReadOnlyMetadataContext? metadata)
        {
            _commandManager?.ShouldEqual(commandManager);
            return TryGetCommand?.Invoke(request, metadata);
        }

        #endregion
    }
}