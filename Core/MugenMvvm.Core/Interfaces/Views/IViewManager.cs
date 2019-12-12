using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Views;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManager : IComponentOwner<IViewManager>, IComponent<IMugenApplication>
    {
        IReadOnlyList<IView> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IViewModelViewMapping> GetMappingByView(object view, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IViewModelViewMapping> GetMappingByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null);

        Task<ViewInitializationResult> InitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel,
            IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default);

        Task CleanupAsync(IView view, IViewModelBase? viewModel, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default);
    }
}