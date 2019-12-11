using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views
{
    public sealed class ViewManager : ComponentOwnerBase<IViewManager>, IViewManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IView> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return GetComponents<IViewProviderComponent>(metadata).TryGetViews(viewModel, metadata) ?? Default.EmptyArray<IView>();
        }

        public IReadOnlyList<IViewModelViewMapping> GetMappingByView(object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            return GetComponents<IViewModelViewMappingProviderComponent>(metadata).TryGetMappingByView(view, metadata) ?? Default.EmptyArray<IViewModelViewMapping>();
        }

        public IReadOnlyList<IViewModelViewMapping> GetMappingByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return GetComponents<IViewModelViewMappingProviderComponent>(metadata).TryGetMappingByViewModel(viewModel, metadata) ?? Default.EmptyArray<IViewModelViewMapping>();
        }

        public Task<ViewInitializationResult> InitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            var task = GetComponents<IViewInitializerComponent>(metadata).TryInitializeAsync(mapping, view, viewModel, cancellationToken, metadata);
            if (task == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return task;
        }

        public Task CleanupAsync(IView view, IViewModelBase? viewModel, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            var task = GetComponents<IViewInitializerComponent>(metadata).TryCleanupAsync(view, viewModel, cancellationToken, metadata);
            if (task == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return task;
        }

        #endregion
    }
}