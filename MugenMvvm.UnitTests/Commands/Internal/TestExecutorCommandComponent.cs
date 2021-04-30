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
        private readonly ICompositeCommand? _command;

        public TestCommandExecutorComponent(ICompositeCommand? command)
        {
            _command = command;
        }

        public Func<object?, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>>? ExecuteAsync { get; set; }

        ValueTask<bool> ICommandExecutorComponent.ExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _command?.ShouldEqual(command);
            return ExecuteAsync?.Invoke(parameter, cancellationToken, metadata) ?? default;
        }
    }
}