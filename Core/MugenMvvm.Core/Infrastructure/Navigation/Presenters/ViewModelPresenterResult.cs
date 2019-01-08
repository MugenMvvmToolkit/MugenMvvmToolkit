using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenterResult : IViewModelPresenterResult
    {
        #region Constructors

        public ViewModelPresenterResult(IReadOnlyMetadataContext metadata, INavigationCallback showingCallback, INavigationCallback closeCallback)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(showingCallback, nameof(showingCallback));
            Should.NotBeNull(closeCallback, nameof(closeCallback));
            Metadata = metadata;
            ShowingCallback = showingCallback;
            CloseCallback = closeCallback;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public INavigationCallback ShowingCallback { get; }

        public INavigationCallback CloseCallback { get; }

        #endregion
    }
}