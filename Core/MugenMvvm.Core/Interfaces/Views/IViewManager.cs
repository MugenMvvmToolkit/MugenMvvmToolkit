using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManager : IHasListeners<IViewManagerListener>
    {
        Task<object> GetViewAsync(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext? context = null);

        Task InitializeViewAsync(object view, IViewModel viewModel, IReadOnlyMetadataContext? context = null);

        Task CleanupViewAsync(IViewModel viewModel, IReadOnlyMetadataContext? context = null);
    }
}