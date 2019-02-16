using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters.Results
{
    public interface IViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        IViewModelBase ViewModel { get; }

        INavigationCallback ShowingCallback { get; }

        INavigationCallback CloseCallback { get; }
    }
}