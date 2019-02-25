using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationMediator : INavigationProvider
    {
        NavigationType NavigationType { get; }

        bool IsOpen { get; }

        IViewInfo? ViewInfo { get; }

        IViewModelBase ViewModel { get; }

        IViewInitializer ViewInitializer { get; }

        void Initialize(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Show(IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Close(IReadOnlyMetadataContext metadata);

        IReadOnlyMetadataContext Restore(IViewInfo viewInfo, IReadOnlyMetadataContext metadata);
    }
}