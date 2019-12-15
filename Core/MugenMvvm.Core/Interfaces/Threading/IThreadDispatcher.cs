using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher : IComponent<IMugenApplication>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode);

        void Execute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state = default, IReadOnlyMetadataContext? metadata = null);

        void Execute<TState>(ThreadExecutionMode executionMode, Action<TState> handler, TState state = default, IReadOnlyMetadataContext? metadata = null);
    }
}