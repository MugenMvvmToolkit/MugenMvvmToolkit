using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;

namespace MugenMvvm.Infrastructure.Presenters.Results
{
    public class RestorationViewModelPresenterResult : IRestorationViewModelPresenterResult
    {
        #region Fields

        public static readonly IRestorationViewModelPresenterResult Unrestored;

        #endregion

        #region Constructors

        static RestorationViewModelPresenterResult()
        {
            Unrestored = new RestorationViewModelPresenterResult(Default.MetadataContext, false);
        }

        public RestorationViewModelPresenterResult(IReadOnlyMetadataContext metadata, bool isRestored)
        {
            Metadata = metadata;
            IsRestored = isRestored;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public bool IsRestored { get; }

        #endregion
    }
}