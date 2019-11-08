using System;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher : IComponent<IMugenApplication>
    {
        bool CanExecuteInline(ThreadExecutionMode executionMode);

        void Execute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state = default,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        void Execute<TState>(ThreadExecutionMode executionMode, Action<TState> action, TState state = default,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}