using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContextFactory : IAttachableComponent<INavigationDispatcher>
    {
        INavigationContext GetNavigationContext(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom, IViewModelBase? viewModelFrom,
            NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext metadata);

        INavigationContext GetNavigationContextFrom(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType, IViewModelBase? viewModel,
            IReadOnlyMetadataContext metadata);

        INavigationContext GetNavigationContextTo(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType, IViewModelBase? viewModel,
            IReadOnlyMetadataContext metadata);
    }
}