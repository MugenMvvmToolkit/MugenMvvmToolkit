using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Views;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewInitializerComponent : IComponent<IViewManager>
    {
        Task<ViewInitializationResult>? TryInitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        Task? TryCleanupAsync(IView view, IViewModelBase? viewModel, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}