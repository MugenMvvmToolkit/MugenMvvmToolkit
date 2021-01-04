using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Commands.Internal
{
    public class TestCommandExecutorComponent : ICommandExecutorComponent
    {
        #region Fields

        private readonly ICompositeCommand? _command;

        #endregion

        #region Constructors

        public TestCommandExecutorComponent(ICompositeCommand? command)
        {
            _command = command;
        }

        #endregion

        #region Properties

        public Func<object?, CancellationToken, IReadOnlyMetadataContext?, Task>? ExecuteAsync { get; set; }

        #endregion

        #region Implementation of interfaces

        Task ICommandExecutorComponent.ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _command?.ShouldEqual(command);
            return ExecuteAsync?.Invoke(parameter, cancellationToken, metadata) ?? Default.CompletedTask;
        }

        #endregion
    }
}