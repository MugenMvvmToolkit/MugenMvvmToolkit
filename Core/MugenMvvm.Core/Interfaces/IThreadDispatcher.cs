using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IThreadDispatcher
    {
        bool IsOnMainThread { get; }

        void Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state);

        void Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state);        
    }
}