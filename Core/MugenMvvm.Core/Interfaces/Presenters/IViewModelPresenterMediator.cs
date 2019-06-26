using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IViewModelPresenterMediator
    {
        NavigationType NavigationType { get; }

        bool IsOpen { get; }

        IViewInfo? ViewInfo { get; }

        IViewModelBase ViewModel { get; }

        IViewInitializer ViewInitializer { get; }

        void Initialize(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata);

        IPresenterResult Show(IReadOnlyMetadataContext metadata);

        IPresenterResult Close(IReadOnlyMetadataContext metadata);

        IPresenterResult Restore(IViewInfo viewInfo, IReadOnlyMetadataContext metadata);
    }
}