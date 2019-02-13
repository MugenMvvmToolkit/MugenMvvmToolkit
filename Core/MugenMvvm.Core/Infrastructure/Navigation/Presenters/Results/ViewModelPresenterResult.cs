using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Infrastructure.Navigation.Presenters.Results
{
    public class ViewModelPresenterResult : IViewModelPresenterResult
    {
        #region Constructors

        public ViewModelPresenterResult(INavigationCallback showingCallback, INavigationCallback closeCallback, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(showingCallback, nameof(showingCallback));
            Should.NotBeNull(closeCallback, nameof(closeCallback));
            Should.NotBeNull(metadata, nameof(metadata));
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