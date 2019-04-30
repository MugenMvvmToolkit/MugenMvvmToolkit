using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContextFactory : IHasListeners<INavigationContextFactoryListener>, IAttachableComponent<INavigationDispatcher>,
        IDetachableComponent<INavigationDispatcher>
    {
        INavigationContext GetNavigationContext(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
            IViewModelBase? viewModelFrom,
            NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext metadata);

        INavigationContext GetNavigationContextFrom(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType, IViewModelBase? viewModel,
            IReadOnlyMetadataContext metadata);

        INavigationContext GetNavigationContextTo(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType, IViewModelBase? viewModel,
            IReadOnlyMetadataContext metadata);
    }
}