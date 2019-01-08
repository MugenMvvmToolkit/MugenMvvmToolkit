using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ClosingViewModelPresenterResult : IClosingViewModelPresenterResult
    {
        #region Fields

        public static readonly IClosingViewModelPresenterResult FalseResult;

        #endregion

        #region Constructors

        static ClosingViewModelPresenterResult()
        {
            var callback = new NavigationCallback(NavigationCallbackType.Closing, NavigationType.Undefined, false);
            ((INavigationCallbackInternal) callback).SetResult(false, null);
            FalseResult = new ClosingViewModelPresenterResult(Default.MetadataContext, callback);
        }

        public ClosingViewModelPresenterResult(IReadOnlyMetadataContext metadata, INavigationCallback<bool> closingCallback)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(closingCallback, nameof(closingCallback));
            Metadata = metadata;
            ClosingCallback = closingCallback;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public INavigationCallback<bool> ClosingCallback { get; }

        #endregion
    }
}