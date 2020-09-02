using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Android.Presenters
{
    public class FragmentDialogViewPresenter : ViewPresenterBase<IDialogFragmentView>
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public FragmentDialogViewPresenter(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Popup;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        #endregion

        #region Methods

        protected override void Activate(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext)
        {
        }

        protected override void Show(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode == NavigationMode.New)
            {
                view.Cancelable = navigationContext.GetMetadataOrDefault().Get(NavigationMetadata.NonModal);
                FragmentExtensions.Show(view, NavigationDispatcher.GetTopView<IActivityView>()!, null!);
            }
        }

        protected override void Close(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext) => view.DismissAllowingStateLoss();

        #endregion
    }
}