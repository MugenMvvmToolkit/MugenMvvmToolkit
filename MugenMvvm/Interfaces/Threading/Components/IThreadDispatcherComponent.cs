using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading.Components
{
    public interface IThreadDispatcherComponent : IComponent<IThreadDispatcher>
    {
        bool CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);

        bool TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata);
    }
}