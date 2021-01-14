using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ExecutionModeViewManagerDecorator : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent
    {
        private readonly IThreadDispatcher? _threadDispatcher;

        [Preserve(Conditional = true)]
        public ExecutionModeViewManagerDecorator(IThreadDispatcher? threadDispatcher = null, int priority = ViewComponentPriority.ExecutionModeDecorator)
            : base(priority)
        {
            _threadDispatcher = threadDispatcher;
            InitializeExecutionMode = ThreadExecutionMode.Main;
            CleanupExecutionMode = ThreadExecutionMode.Main;
        }

        public ThreadExecutionMode InitializeExecutionMode { get; set; }

        public ThreadExecutionMode CleanupExecutionMode { get; set; }

        public ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(InitializeExecutionMode, metadata))
                return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

            var tcs = new TaskCompletionSource<IView?>();
            dispatcher.Execute(InitializeExecutionMode, s =>
            {
                var state =
                    (Tuple<ExecutionModeViewManagerDecorator, IViewManager, TaskCompletionSource<IView?>, IViewMapping, object, CancellationToken, IReadOnlyMetadataContext?>) s!;
                if (state.Item6.IsCancellationRequested)
                {
                    state.Item3.TrySetCanceled(state.Item6);
                    return;
                }

                var task = state.Item1.Components.TryInitializeAsync(state.Item2, state.Item4, state.Item5, state.Item6, state.Item7);
                state.Item3.TrySetFromTask(task);
            }, Tuple.Create(this, viewManager, tcs, mapping, request, cancellationToken, metadata), metadata);
            return tcs.Task.AsValueTask();
        }

        public ValueTask<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(CleanupExecutionMode, metadata))
                return Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);

            var tcs = new TaskCompletionSource<bool>();
            dispatcher.Execute(CleanupExecutionMode, s =>
            {
                var tuple = (Tuple<ExecutionModeViewManagerDecorator, IViewManager, TaskCompletionSource<bool>, IView, object?, CancellationToken, IReadOnlyMetadataContext?>) s!;
                if (tuple.Item6.IsCancellationRequested)
                {
                    tuple.Item3.TrySetCanceled(tuple.Item6);
                    return;
                }

                var task = tuple.Item1.Components.TryCleanupAsync(tuple.Item2, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7);
                tuple.Item3.TrySetFromTask(task);
            }, Tuple.Create(this, viewManager, tcs, view, state, cancellationToken, metadata), metadata);
            return tcs.Task.AsValueTask();
        }
    }
}