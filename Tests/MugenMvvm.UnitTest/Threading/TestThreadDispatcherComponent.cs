using System;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;

namespace MugenMvvm.UnitTest.Threading
{
    public sealed class TestThreadDispatcherComponent : IThreadDispatcherComponent, IHasPriority
    {
        #region Properties

        public Func<ThreadExecutionMode, bool>? CanExecuteInline { get; set; }

        public Func<Action<object?>, ThreadExecutionMode, object?, Type, IReadOnlyMetadataContext?, bool> Execute { get; set; } = (action, _, state, __, ___) =>
        {
            action(state);
            return true;
        };

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        bool IThreadDispatcherComponent.CanExecuteInline(ThreadExecutionMode executionMode)
        {
            return CanExecuteInline?.Invoke(executionMode) ?? true;
        }

        bool IThreadDispatcherComponent.TryExecute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            return Execute(handler.Execute!, executionMode, state, typeof(TState), metadata);
        }

        bool IThreadDispatcherComponent.TryExecute<TState>(ThreadExecutionMode executionMode, Action<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            return Execute(o => handler((TState) o!), executionMode, state, typeof(TState), metadata);
        }

        #endregion
    }
}