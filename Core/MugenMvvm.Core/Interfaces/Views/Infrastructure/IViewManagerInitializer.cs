using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerInitializer : IHasMetadata<IReadOnlyMetadataContext>
    {
        IViewManagerResult<IViewInfo> Initialize(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}