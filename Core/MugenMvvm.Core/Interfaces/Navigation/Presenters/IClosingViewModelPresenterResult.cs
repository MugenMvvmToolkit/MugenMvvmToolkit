using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IClosingViewModelPresenterResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        INavigationCallback<bool> ClosingCallback { get; }
    }
}