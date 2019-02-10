using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;

namespace MugenMvvm.Infrastructure.Navigation.Presenters.Results
{
    public class RestorationViewModelPresenterResult : IRestorationViewModelPresenterResult
    {
        #region Fields

        public static readonly IRestorationViewModelPresenterResult Unrestored;

        #endregion

        #region Constructors

        static RestorationViewModelPresenterResult()
        {
            Unrestored = new RestorationViewModelPresenterResult(Default.MetadataContext, NavigationType.Undefined, false);
        }

        public RestorationViewModelPresenterResult(IReadOnlyMetadataContext metadata, NavigationType navigationType, bool isRestored)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Metadata = metadata;
            NavigationType = navigationType;
            IsRestored = isRestored;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public NavigationType NavigationType { get; }

        public bool IsRestored { get; }

        #endregion
    }
}