using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationWindowMediator//todo check view, clear after close
    {
        NavigationType NavigationType { get; }

        bool IsOpen { get; }

        object? View { get; }

        IViewModelBase ViewModel { get; }

        void Initialize(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Show(IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Close(IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Restore(object view, IReadOnlyMetadataContext metadata);
    }
}