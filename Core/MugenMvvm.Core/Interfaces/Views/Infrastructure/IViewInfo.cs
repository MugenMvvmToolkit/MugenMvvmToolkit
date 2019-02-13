using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewInfo : IHasMetadata<IObservableMetadataContext>//todo wrap
    {
        object View { get; }

        Task<ICleanupViewManagerResult> CleanupAsync(IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}