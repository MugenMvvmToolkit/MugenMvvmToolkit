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

        void Execute(ThreadExecutionMode executionMode, IThreadDispatcherHandler handler, object? state = null,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        void Execute(ThreadExecutionMode executionMode, Action<object?> action, object? state = null,
            CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}