using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IApplicationStateAwareNavigationProvider : INavigationProvider
    {
        bool IsSupported(IViewModelBase viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);

        INavigationContext? TryCreateApplicationStateContext(IViewModelBase viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);
    }
}