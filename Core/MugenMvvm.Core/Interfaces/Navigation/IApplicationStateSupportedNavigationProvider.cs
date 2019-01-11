using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IApplicationStateSupportedNavigationProvider
    {
        bool IsSupported(IViewModel viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);

        INavigationContext? TryCreateApplicationStateContext(IViewModel viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata);
    }
}