using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher : IComponentOwner<IThreadDispatcher>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null);

        bool TryExecute(ThreadExecutionMode executionMode, object handler, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}