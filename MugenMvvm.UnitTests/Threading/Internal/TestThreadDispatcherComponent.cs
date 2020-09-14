using System;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using Should;

namespace MugenMvvm.UnitTests.Threading.Internal
{
    public sealed class TestThreadDispatcherComponent : IThreadDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IThreadDispatcher? _dispatcher;

        #endregion

        #region Constructors

        public TestThreadDispatcherComponent(IThreadDispatcher? dispatcher = null)
        {
            _dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        public Func<ThreadExecutionMode, IReadOnlyMetadataContext?, bool>? CanExecuteInline { get; set; }

        public Func<ThreadExecutionMode, object, object?, IReadOnlyMetadataContext?, bool>? TryExecute { get; set; }

        public Func<Action<object?>, ThreadExecutionMode, object?, IReadOnlyMetadataContext?, bool> Execute { get; set; } = (action, _, state, ___) =>
        {
            action(state);
            return true;
        };

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        bool IThreadDispatcherComponent.CanExecuteInline(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            _dispatcher?.ShouldEqual(threadDispatcher);
            return CanExecuteInline?.Invoke(executionMode, metadata) ?? true;
        }

        bool IThreadDispatcherComponent.TryExecute(IThreadDispatcher threadDispatcher, ThreadExecutionMode executionMode, object handler, object? state, IReadOnlyMetadataContext? metadata)
        {
            _dispatcher?.ShouldEqual(threadDispatcher);
            if (TryExecute != null)
                return TryExecute(executionMode, handler!, state, metadata);
            Action<object?> del;
            if (handler is Action action)
                del = o => action();
            else if (handler is Action<object?> actionState)
                del = actionState.Invoke!;
            else if (handler is IThreadDispatcherHandler h)
                del = h.Execute;
            else
                return false;
            return Execute(del, executionMode, state, metadata);
        }

        #endregion
    }
}