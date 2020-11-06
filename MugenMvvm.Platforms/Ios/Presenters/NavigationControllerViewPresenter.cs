using System;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;
using MugenMvvm.Requests;
using UIKit;

namespace MugenMvvm.Ios.Presenters
{
    public class NavigationControllerViewPresenter : ViewPresenterBase<UIViewController>
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IPresenter? _presenter;
        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public NavigationControllerViewPresenter(UINavigationController navigationController, IViewManager? viewManager = null, IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            Should.NotBeNull(navigationController, nameof(navigationController));
            NavigationController = navigationController;
            _viewManager = viewManager;
            _presenter = presenter;
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Page;

        public bool Animated { get; set; } = true;

        public UINavigationController NavigationController { get; }

        public Func<IPresenter, IViewModelBase, IViewMapping, IReadOnlyMetadataContext?, bool>? CanPresentHandler { get; set; }

        protected IPresenter Presenter => _presenter.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        #endregion

        #region Methods

        protected override bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) =>
            base.CanPresent(presenter, viewModel, mapping, metadata) && (CanPresentHandler == null || CanPresentHandler(presenter, viewModel, mapping, metadata));

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext) => ShowInternalAsync(true, mediator, view, navigationContext);

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext) => ShowInternalAsync(false, mediator, view, navigationContext);

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext)
        {
            var controllers = NavigationController.ViewControllers;
            if (controllers == null)
                return Default.CompletedTask;

            var animated = navigationContext.GetOrDefault(NavigationMetadata.Animated, Animated);
            if (controllers.Length != 1 && Equals(NavigationController.TopViewController, view))
            {
                NavigationController.PopViewController(animated);
                return Default.CompletedTask;
            }

            var index = Array.IndexOf(controllers, view);
            if (index < 0)
                return Default.CompletedTask;

            var cancelableRequest = new CancelableRequest(state: this);
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, cancelableRequest, navigationContext.GetMetadataOrDefault());
            if (cancelableRequest.Cancel.GetValueOrDefault())
                return Default.CompletedTask;

            Array.Copy(controllers, index + 1, controllers, index, controllers.Length - index - 1);
            Array.Resize(ref controllers, controllers.Length - 1);
            NavigationController.SetViewControllers(controllers, animated);
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed, null, navigationContext.GetMetadataOrDefault());
            return Default.CompletedTask;
        }

        private async Task ShowInternalAsync(bool bringToFront, IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext)
        {
            var metadata = navigationContext.GetMetadataOrDefault();
            if (metadata.Get(NavigationMetadata.ClearBackStack))
                await NavigationDispatcher.ClearBackStackAsync(NavigationType, mediator.ViewModel, false, metadata, Presenter);

            var animated = metadata.Get(NavigationMetadata.Animated, Animated);
            if (!bringToFront)
            {
                NavigationController.PushViewController(view, animated);
                return;
            }

            if (Equals(NavigationController.TopViewController, view))
                return;

            var controllers = NavigationController.ViewControllers;
            if (controllers == null)
                return;

            var index = Array.IndexOf(controllers, view);
            if (index < 0)
                return;

            Array.Copy(controllers, index + 1, controllers, index, controllers.Length - index - 1);
            controllers[controllers.Length - 1] = view;
            NavigationController.SetViewControllers(controllers, animated);
        }

        #endregion
    }
}