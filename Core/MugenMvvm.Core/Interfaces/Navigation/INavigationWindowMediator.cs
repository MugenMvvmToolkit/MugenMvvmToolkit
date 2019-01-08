using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationWindowMediator
    {
        NavigationType NavigationType { get; }

        bool IsOpen { get; }

        object? View { get; }

        IViewModel ViewModel { get; }

        void Initialize(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        void UpdateView(object? view, bool isOpened, IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Show(IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Close(IReadOnlyMetadataContext metadata);
    }
}