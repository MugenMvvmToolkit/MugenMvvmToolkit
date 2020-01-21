﻿using System;
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

        public Func<ThreadExecutionMode, bool>? CanExecute { get; set; }

        public Action<Action<object?>, ThreadExecutionMode, object?, Type, IReadOnlyMetadataContext?> Execute { get; set; } = (action, _, state, __, ___) => action(state);

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        bool IThreadDispatcherComponent.CanExecuteInline(ThreadExecutionMode executionMode)
        {
            return CanExecute?.Invoke(executionMode) ?? true;
        }

        bool IThreadDispatcherComponent.TryExecute<TState>(ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            Execute(handler.Execute!, executionMode, state, typeof(TState), metadata);
            return true;
        }

        bool IThreadDispatcherComponent.TryExecute<TState>(ThreadExecutionMode executionMode, Action<TState> handler, TState state, IReadOnlyMetadataContext? metadata)
        {
            Execute(o => handler((TState)o!), executionMode, state, typeof(TState), metadata);
            return true;
        }

        #endregion
    }
}