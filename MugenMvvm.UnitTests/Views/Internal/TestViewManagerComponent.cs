using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using Should;

namespace MugenMvvm.UnitTests.Views.Internal
{
    public class TestViewManagerComponent : IViewManagerComponent, IHasPriority
    {
        private readonly IViewManager? _viewManager;

        public TestViewManagerComponent(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        public Func<IViewMapping, object, IReadOnlyMetadataContext?, CancellationToken, ValueTask<IView?>>? TryInitializeAsync { get; set; }

        public Func<IView, object?, IReadOnlyMetadataContext?, CancellationToken, ValueTask<bool>?>? TryCleanupAsync { get; set; }

        public int Priority { get; set; }

        ValueTask<IView?> IViewManagerComponent.TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            return TryInitializeAsync?.Invoke(mapping, request!, metadata, cancellationToken) ?? default;
        }

        ValueTask<bool> IViewManagerComponent.TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            return TryCleanupAsync?.Invoke(view, state, metadata, cancellationToken) ?? default;
        }
    }
}