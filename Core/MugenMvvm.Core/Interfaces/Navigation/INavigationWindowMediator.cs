using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationWindowMediator
    {
        bool IsOpen { get; }

        object? View { get; }

        IViewModel ViewModel { get; }

        void Initialize(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        void UpdateView(object? view, bool isOpened, IReadOnlyMetadataContext metadata);

        void Show(IReadOnlyMetadataContext metadata);

        Task<bool> CloseAsync(IReadOnlyMetadataContext metadata);
    }
}