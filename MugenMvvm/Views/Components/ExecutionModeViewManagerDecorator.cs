using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ExecutionModeViewManagerDecorator : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Fields

        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExecutionModeViewManagerDecorator(IThreadDispatcher? threadDispatcher = null)
        {
            _threadDispatcher = threadDispatcher;
            InitializeExecutionMode = ThreadExecutionMode.Main;
            CleanupExecutionMode = ThreadExecutionMode.Main;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ExecutionModeDecorator;

        public ThreadExecutionMode InitializeExecutionMode { get; set; }

        public ThreadExecutionMode CleanupExecutionMode { get; set; }

        #endregion

        #region Implementation of interfaces

        public Task<IView>? TryInitializeAsync<TRequest>(IViewManager viewManager, IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(InitializeExecutionMode, metadata))
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

            var tcs = new TaskCompletionSource<IView>();
            var valueTuple = (this, viewManager, tcs, mapping, request, cancellationToken, metadata);
            dispatcher.Execute(InitializeExecutionMode, valueTuple, state =>
            {
                try
                {
                    if (state.cancellationToken.IsCancellationRequested)
                    {
                        state.tcs.TrySetCanceled(state.cancellationToken);
                        return;
                    }

                    var task = state.Item1.Components.TryInitializeAsync(state.viewManager, state.mapping, state.request!, state.cancellationToken, state.metadata);
                    if (task == null)
                        ExceptionManager.ThrowObjectNotInitialized(state.Item1.Components);
                    state.tcs.TrySetFromTask(task);
                }
                catch (Exception e)
                {
                    state.tcs.TrySetException(e);
                }
            }, metadata);
            return tcs.Task;
        }

        public Task? TryCleanupAsync<TRequest>(IViewManager viewManager, IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(CleanupExecutionMode, metadata))
                return Components.TryCleanupAsync(viewManager, view, request, cancellationToken, metadata);

            var tcs = new TaskCompletionSource<object?>();
            var valueTuple = (this, viewManager, tcs, view, request, cancellationToken, metadata);
            dispatcher.Execute(CleanupExecutionMode, valueTuple, state =>
            {
                try
                {
                    if (state.cancellationToken.IsCancellationRequested)
                    {
                        state.tcs.TrySetCanceled(state.cancellationToken);
                        return;
                    }

                    var task = state.Item1.Components.TryCleanupAsync(state.viewManager, state.view, state.request, state.cancellationToken, state.metadata);
                    if (task == null)
                        ExceptionManager.ThrowObjectNotInitialized(state.Item1);
                    state.tcs.TrySetFromTask(task);
                }
                catch (Exception e)
                {
                    state.tcs.TrySetException(e);
                }
            }, metadata);
            return tcs.Task;
        }

        #endregion
    }
}