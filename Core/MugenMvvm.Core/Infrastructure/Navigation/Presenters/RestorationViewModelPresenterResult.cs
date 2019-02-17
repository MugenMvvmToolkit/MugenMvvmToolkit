using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class RestorationViewModelPresenterResult : ChildViewModelPresenterResult, IRestorationViewModelPresenterResult
    {
        #region Fields

        public static readonly IRestorationViewModelPresenterResult Unrestored;

        #endregion

        #region Constructors

        static RestorationViewModelPresenterResult()
        {
            Unrestored = new RestorationViewModelPresenterResult(false, Default.NavigationProvider, NavigationType.Undefined, Default.MetadataContext, null);
        }

        public RestorationViewModelPresenterResult(bool isRestored, INavigationProvider navigationProvider, NavigationType navigationType,
            IReadOnlyMetadataContext metadata, IChildViewModelPresenter? presenter)
            : base(navigationProvider, navigationType, metadata, presenter)
        {
            IsRestored = isRestored;
        }

        public RestorationViewModelPresenterResult(bool isRestored, IChildViewModelPresenterResult childResult)
            : this(isRestored, childResult.NavigationProvider, childResult.NavigationType, childResult.Metadata, childResult.Presenter)
        {
        }

        #endregion

        #region Properties

        public bool IsRestored { get; }

        #endregion
    }
}