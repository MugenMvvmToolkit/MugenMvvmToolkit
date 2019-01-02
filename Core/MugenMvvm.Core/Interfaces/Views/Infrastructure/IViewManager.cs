using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManager : IHasListeners<IViewManagerListener>
    {
        Task<object> GetViewAsync(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata);

        Task InitializeViewAsync(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata);

        Task CleanupViewAsync(IViewModel viewModel, IReadOnlyMetadataContext metadata);
    }
}