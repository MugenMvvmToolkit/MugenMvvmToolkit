using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Tests.Commands
{
    public class TestCommandExecutorComponent : ICommandExecutorComponent, IHasPriority
    {
        public Func<ICompositeCommand, object?, CancellationToken, IReadOnlyMetadataContext?, Task<bool>>? ExecuteAsync { get; set; }

        public int Priority { get; set; }

        Task<bool> ICommandExecutorComponent.TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) => ExecuteAsync?.Invoke(command, parameter, cancellationToken, metadata) ?? Default.FalseTask;
    }
}