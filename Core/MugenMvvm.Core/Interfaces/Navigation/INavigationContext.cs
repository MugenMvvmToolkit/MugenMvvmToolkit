using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext
    {
        IMetadataContext Metadata { get; }

        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }

        object? Provider { get; }

        IViewModel? ViewModelFrom { get; }

        IViewModel? ViewModelTo { get; }
    }
}