using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Tests.Views
{
    public class TestViewManagerComponent : IViewManagerComponent, IHasPriority
    {
        public Func<IViewManager, IViewMapping, object, IReadOnlyMetadataContext?, CancellationToken, ValueTask<IView?>>? TryInitializeAsync { get; set; }

        public Func<IViewManager, IView, object?, IReadOnlyMetadataContext?, CancellationToken, Task<bool>>? TryCleanupAsync { get; set; }

        public int Priority { get; set; }

        ValueTask<IView?> IViewManagerComponent.TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) =>
            TryInitializeAsync?.Invoke(viewManager, mapping, request!, metadata, cancellationToken) ?? default;

        Task<bool> IViewManagerComponent.TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) =>
            TryCleanupAsync?.Invoke(viewManager, view, state, metadata, cancellationToken) ?? Default.FalseTask;
    }
}