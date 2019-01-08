using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        INavigationCallback ShowingCallback { get; }

        INavigationCallback CloseCallback { get; }
    }
}