using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CanExecuteCommandCondition : ComponentDecoratorBase<ICompositeCommand, ICommandExecutorComponent>, ICommandExecutorComponent
    {
        public CanExecuteCommandCondition(int priority = CommandComponentPriority.CanExecuteCondition) : base(priority)
        {
        }

        public bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => Components.IsExecuting(command, metadata);

        public Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (command.CanExecute(parameter, metadata))
                return Components.TryExecuteAsync(command, parameter, cancellationToken, metadata);
            return Default.FalseTask;
        }

        public Task TryWaitAsync(ICompositeCommand command, IReadOnlyMetadataContext? metadata) => Components.TryWaitAsync(command, metadata);
    }
}