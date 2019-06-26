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

        void Initialize(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext? metadata = null);

        IPresenterResult Show(IReadOnlyMetadataContext? metadata = null);

        IPresenterResult Close(IReadOnlyMetadataContext? metadata = null);

        IPresenterResult Restore(IViewInfo viewInfo, IReadOnlyMetadataContext? metadata = null);
    }
}