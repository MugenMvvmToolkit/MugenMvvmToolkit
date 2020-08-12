using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Android.Presenters
{
    public class FragmentDialogViewPresenter : ViewPresenterBase<IDialogFragmentView>
    {
        #region Constructors

        public FragmentDialogViewPresenter(INavigationDispatcher? navigationDispatcher = null)
            : base(navigationDispatcher)
        {
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Window;

        #endregion

        #region Methods

        public override Task WaitBeforeCloseAsync(IViewModelPresenterMediator mediator, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close, metadata);

        protected override Task WaitBeforeShowAsync(IViewModelPresenterMediator mediator, IDialogFragmentView? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => (callback.NavigationType == state.NavigationType || callback.NavigationType == NavigationType.Page)
                                                                                   && (callback.CallbackType == NavigationCallbackType.Showing || callback.CallbackType == NavigationCallbackType.Closing), metadata);

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