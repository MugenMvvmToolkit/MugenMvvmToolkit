using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IHasMetadata<IMetadataContext>
    {
        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }

        object? NavigationProvider { get; }

        IViewModel? ViewModelFrom { get; }

        IViewModel? ViewModelTo { get; }
    }
}