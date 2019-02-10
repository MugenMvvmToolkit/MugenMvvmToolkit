using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewInfo : IHasMetadata<IObservableMetadataContext>
    {
        object View { get; }

        IViewManagerCleanupResult Cleanup(IViewModel viewModel, IReadOnlyMetadataContext metadata);
    }
}