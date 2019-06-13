using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ChildViewModelPresenterResult : IChildViewModelPresenterResult
    {
        #region Constructors

        public ChildViewModelPresenterResult(INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext metadata,
            IChildViewModelPresenter? presenter)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            NavigationProvider = navigationProvider;
            Presenter = presenter;
            Metadata = metadata;
            NavigationType = navigationType;
        }

        #endregion

        #region Properties

        public bool IsMetadataInitialized => true;

        public INavigationProvider NavigationProvider { get; }

        public IChildViewModelPresenter? Presenter { get; }

        public IReadOnlyMetadataContext Metadata { get; }

        public NavigationType NavigationType { get; }

        #endregion

        #region Methods

        public static IChildViewModelPresenterResult CreateShowResult(INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext metadata,
            IChildViewModelPresenter presenter, IMetadataContextProvider metadataContextProvider, bool? isRestorableCallback = null)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            var resultMetadata = metadataContextProvider.GetMetadataContext(presenter, metadata);
            resultMetadata.Set(NavigationInternalMetadata.IsRestorableCallback, isRestorableCallback.GetValueOrDefault(presenter is IRestorableChildViewModelPresenter));
            return new ChildViewModelPresenterResult(navigationProvider, navigationType, resultMetadata, presenter);
        }

        #endregion
    }
}