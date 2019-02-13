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
            Unrestored = new RestorationViewModelPresenterResult(NavigationType.Undefined, false, Default.MetadataContext);
        }

        public RestorationViewModelPresenterResult(NavigationType navigationType, bool isRestored, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
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