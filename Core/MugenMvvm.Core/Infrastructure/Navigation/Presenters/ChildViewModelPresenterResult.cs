using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ChildViewModelPresenterResult : IChildViewModelPresenterResult
    {
        #region Constructors

        public ChildViewModelPresenterResult(INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            NavigationProvider = navigationProvider;
            Metadata = metadata;
            NavigationType = navigationType;
        }

        #endregion

        #region Properties

        public INavigationProvider NavigationProvider { get; }

        public IReadOnlyMetadataContext Metadata { get; }

        public NavigationType NavigationType { get; }

        #endregion

        #region Methods

        public static IChildViewModelPresenterResult CreateShowResult(INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext metadata,
            IChildViewModelPresenter presenter)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            return CreateShowResult(navigationProvider, navigationType, metadata, presenter is IRestorableChildViewModelPresenter);
        }

        public static IChildViewModelPresenterResult CreateShowResult(INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext metadata, bool isRestorableCallback)
        {
            var resultMetadata = new MetadataContext(metadata);
            resultMetadata.Set(NavigationInternalMetadata.IsRestorableCallback, isRestorableCallback);
            return new ChildViewModelPresenterResult(navigationProvider, navigationType, resultMetadata);
        }

        #endregion
    }
}