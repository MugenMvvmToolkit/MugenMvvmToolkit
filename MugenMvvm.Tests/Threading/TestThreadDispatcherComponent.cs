using System;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.Tests.Threading
{
    public sealed class TestThreadDispatcherComponent : IThreadDispatcherComponent, IHasPriority
    {
        public Func<IThreadDispatcher, ThreadExecutionMode, IReadOnlyMetadataContext?, bool>? CanExecuteInline { get; set; }

        public Func<IThreadDispatcher, ThreadExecutionMode, object, object?, IReadOnlyMetadataContext?, bool>? TryExecute { get; set; }

        public Func<IThreadDispatcher, Action<object?>, ThreadExecutionMode, object?, IReadOnlyMetadataContext?, bool> Execute { get; set; } = (_, action, _, state, _) =>
        {
            action(state);
            return true;
        };

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        bool IThreadDispatcherComponent.CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata) =>
            CanExecuteInline?.Invoke(threadDispatcher, executionMode, metadata) ?? true;

        bool IThreadDispatcherComponent.TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            if (TryExecute != null)
                return TryExecute(threadDispatcher, executionMode, handler!, state, metadata);
            Action<object?> del;
            if (handler is Action action)
                del = o => action();
            else if (handler is Action<object?> actionState)
                del = actionState.Invoke!;
            else if (handler is IThreadDispatcherHandler h)
                del = h.Execute;
            else
                return false;
            return Execute(threadDispatcher, del, executionMode, state, metadata);
        }
    }
}