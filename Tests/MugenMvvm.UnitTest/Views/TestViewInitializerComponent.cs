using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Views;

namespace MugenMvvm.UnitTest.Views
{
    public class TestViewInitializerComponent : IViewInitializerComponent, IHasPriority
    {
        #region Properties

        public Func<IViewModelViewMapping, object?, IViewModelBase?, IReadOnlyMetadataContext?, CancellationToken, Task<ViewInitializationResult>?>? TryInitializeAsync { get; set; }

        public Func<IView, IViewModelBase?, IReadOnlyMetadataContext?, CancellationToken, Task?>? TryCleanupAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<ViewInitializationResult>? IViewInitializerComponent.TryInitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata,
            CancellationToken cancellationToken)
        {
            return TryInitializeAsync?.Invoke(mapping, view, viewModel, metadata, cancellationToken);
        }

        Task? IViewInitializerComponent.TryCleanupAsync(IView view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken)
        {
            return TryCleanupAsync?.Invoke(view, viewModel, metadata, cancellationToken);
        }

        #endregion
    }
}