using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading.Components
{
    public interface IThreadDispatcherComponent : IComponent<IThreadDispatcher>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode);

        bool TryExecute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state, IReadOnlyMetadataContext? metadata);

        bool TryExecute<TState>(ThreadExecutionMode executionMode, Action<TState> handler, TState state, IReadOnlyMetadataContext? metadata);
    }
}