using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewMapping;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManager : IHasListeners<IViewManagerListener>
    {
        Task<object> GetViewAsync(IViewMappingItem viewMapping, IReadOnlyMetadataContext? context = null);

        Task InitializeViewAsync(IViewModel viewModel, object view, IReadOnlyMetadataContext? context = null);

        Task CleanupViewAsync(IViewModel viewModel, IReadOnlyMetadataContext? context = null);
    }
}