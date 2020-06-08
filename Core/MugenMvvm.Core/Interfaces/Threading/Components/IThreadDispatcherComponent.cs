using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading.Components
{
    public interface IThreadDispatcherComponent : IComponent<IThreadDispatcher>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);

        bool TryExecute<TState>(ThreadExecutionMode executionMode, object handler, in TState state, IReadOnlyMetadataContext? metadata);
    }
}