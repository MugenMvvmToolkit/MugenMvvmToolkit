using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IHasMetadata<IMetadataContext>
    {
        INavigationProvider NavigationProvider { get; }

        NavigationMode NavigationMode { get; }

        NavigationType NavigationTypeFrom { get; }

        NavigationType NavigationTypeTo { get; }

        IViewModelBase? ViewModelFrom { get; }

        IViewModelBase? ViewModelTo { get; }
    }
}