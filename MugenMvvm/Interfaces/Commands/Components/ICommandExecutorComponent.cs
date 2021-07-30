using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandExecutorComponent : IComponent<ICompositeCommand>
    {
        bool IsExecuting(ICompositeCommand command, IReadOnlyMetadataContext? metadata);

        Task<bool> TryExecuteAsync(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}