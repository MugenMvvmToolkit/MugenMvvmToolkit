using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher
    {
        bool IsOnMainThread { get; }

        void Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state, IReadOnlyMetadataContext? metadata = null);

        void Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state, IReadOnlyMetadataContext? metadata = null);
    }
}