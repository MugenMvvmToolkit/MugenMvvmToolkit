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

        void Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        void Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}