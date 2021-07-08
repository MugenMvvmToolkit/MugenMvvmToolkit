using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Metadata;
using MugenMvvm.Presentation;

namespace MugenMvvm.Android.Presentation
{
    public class FragmentDialogViewPresenterMediator : ViewPresenterMediatorBase<IDialogFragmentView>
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        public FragmentDialogViewPresenterMediator(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public override NavigationType NavigationType => NavigationType.Popup;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext,
            CancellationToken cancellationToken) => Task.CompletedTask;

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.NavigationMode == NavigationMode.New)
            {
                view.Cancelable = !navigationContext.GetOrDefault(NavigationMetadata.Modal);
                FragmentMugenExtensions.Show(view, NavigationDispatcher.GetTopView<IActivityView>(null, true, mediator.ViewModel, navigationContext.GetMetadataOrDefault())!,
                    null!);
            }

            return Task.CompletedTask;
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, IDialogFragmentView view, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            view.DismissAllowingStateLoss();
            return Task.CompletedTask;
        }
    }
}