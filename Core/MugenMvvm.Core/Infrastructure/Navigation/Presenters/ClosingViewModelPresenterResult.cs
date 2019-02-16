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
            var callback = new NavigationCallback(NavigationCallbackType.Closing, NavigationType.Undefined, false, string.Empty);
            ((INavigationCallbackInternal)callback).SetResult(false, null);
            FalseResult = new ClosingViewModelPresenterResult(callback, Default.MetadataContext);
        }

        public ClosingViewModelPresenterResult(INavigationCallback<bool> closingCallback, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(closingCallback, nameof(closingCallback));
            Should.NotBeNull(metadata, nameof(metadata));
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