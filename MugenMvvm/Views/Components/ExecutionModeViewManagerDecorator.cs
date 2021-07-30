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

        public async ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            await _threadDispatcher.SwitchToAsync(InitializeExecutionMode);
            return await Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata).ConfigureAwait(false);
        }

        public async Task<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            await _threadDispatcher.SwitchToAsync(CleanupExecutionMode);
            return await Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata).ConfigureAwait(false);
        }
    }
}