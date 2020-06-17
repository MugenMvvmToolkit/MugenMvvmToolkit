using System;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.UnitTest.Threading.Internal
{
    public sealed class TestThreadDispatcherComponent : IThreadDispatcherComponent, IHasPriority
    {
        #region Properties

        public Func<ThreadExecutionMode, IReadOnlyMetadataContext?, bool>? CanExecuteInline { get; set; }

        public Func<ThreadExecutionMode, object, object?, Type, IReadOnlyMetadataContext?, bool>? TryExecute { get; set; }

        public Func<Action<object?>, ThreadExecutionMode, object?, IReadOnlyMetadataContext?, bool> Execute { get; set; } = (action, _, state, ___) =>
        {
            action(state);
            return true;
        };

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        bool IThreadDispatcherComponent.CanExecuteInline(ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            return CanExecuteInline?.Invoke(executionMode, metadata) ?? true;
        }

        bool IThreadDispatcherComponent.TryExecute<THandler, TState>(ThreadExecutionMode executionMode, in THandler handler, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (TryExecute != null)
                return TryExecute(executionMode, handler!, state, typeof(TState), metadata);
            Action<object?> del;
            if (handler is Action action)
                del = o => action();
            else if (handler is Action<TState> actionState)
                del = actionState.Invoke!;
            else if (handler is IThreadDispatcherHandler h)
                del = o => h.Execute();
            else if (handler is IThreadDispatcherHandler<TState> handlerState)
                del = handlerState.Execute!;
            else
                return false;
            return Execute(del, executionMode, state, metadata);
        }

        #endregion
    }
}