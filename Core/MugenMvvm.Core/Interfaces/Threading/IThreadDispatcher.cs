using System;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcher
    {
        bool IsOnMainThread { get; }

        void Execute(IThreadDispatcherHandler handler, ThreadExecutionMode executionMode, object? state);

        void Execute(Action<object?> action, ThreadExecutionMode executionMode, object? state);        
    }
}