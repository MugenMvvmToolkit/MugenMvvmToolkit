using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher : IComponentOwner<IThreadDispatcher>, IComponent<IMugenApplication>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null);

        void Execute<TState>(ThreadExecutionMode executionMode, object handler, TState state, IReadOnlyMetadataContext? metadata = null);
    }
}