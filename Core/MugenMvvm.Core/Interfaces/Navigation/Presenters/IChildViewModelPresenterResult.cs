using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IChildViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }

        IChildViewModelPresenter? Presenter { get; }
    }
}