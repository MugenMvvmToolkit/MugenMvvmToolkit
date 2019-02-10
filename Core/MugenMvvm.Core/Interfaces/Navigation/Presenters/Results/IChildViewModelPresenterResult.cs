using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters.Results
{
    public interface IChildViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }
    }
}