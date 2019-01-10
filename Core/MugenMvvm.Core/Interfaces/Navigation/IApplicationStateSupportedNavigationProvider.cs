using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IApplicationStateSupportedNavigationProvider
    {
        bool IsSupported(IViewModel viewModel, IReadOnlyMetadataContext metadata);

        INavigationContext? TryCreateApplicationStateContext(IViewModel viewModel, IReadOnlyMetadataContext metadata);
    }
}