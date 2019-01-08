using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ChildViewModelPresenterResult : IChildViewModelPresenterResult
    {
        #region Constructors

        public ChildViewModelPresenterResult(IReadOnlyMetadataContext metadata, NavigationType navigationType)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Metadata = metadata;
            NavigationType = navigationType;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public NavigationType NavigationType { get; }

        #endregion

        #region Methods

        public static IChildViewModelPresenterResult CreateShowResult(NavigationType navigationType, IReadOnlyMetadataContext metadata, IChildViewModelPresenter presenter)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            return CreateShowResult(navigationType, metadata, presenter is IRestorableChildViewModelPresenter);
        }

        public static IChildViewModelPresenterResult CreateShowResult(NavigationType navigationType, IReadOnlyMetadataContext metadata, bool isRestorableCallback)
        {
            var resultMetadata = new MetadataContext(metadata);
            resultMetadata.Set(NavigationInternalMetadata.IsRestorableCallback, isRestorableCallback);
            return new ChildViewModelPresenterResult(resultMetadata, navigationType);
        }

        #endregion
    }
}