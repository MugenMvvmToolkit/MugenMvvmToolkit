using System.Threading.Tasks;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Presentation;

namespace MugenMvvm.Android.Presentation
{
    public class FragmentDialogViewPresenter : ViewPresenterBase<IDialogFragmentView>
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        public FragmentDialogViewPresenter(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public override NavigationType NavigationType => NavigationType.Popup;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext) => Default.CompletedTask;

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode == NavigationMode.New)
            {
                view.Cancelable = navigationContext.GetOrDefault(NavigationMetadata.NonModal);
                FragmentMugenExtensions.Show(view, NavigationDispatcher.GetTopView<IActivityView>()!, null!);
            }

            return Default.CompletedTask;
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext)
        {
            view.DismissAllowingStateLoss();
            return Default.CompletedTask;
        }
    }
}