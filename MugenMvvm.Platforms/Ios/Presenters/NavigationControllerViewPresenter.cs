using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
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

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected IPresenter Presenter => _presenter.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        #endregion

        #region Methods

        protected override bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) =>
            base.CanPresent(presenter, viewModel, mapping, metadata) && (CanPresentHandler == null || CanPresentHandler(presenter, viewModel, mapping, metadata));

        protected override void Activate(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext) => ShowInternal(true, mediator, view, navigationContext);

        protected override void Show(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext) => ShowInternal(false, mediator, view, navigationContext);

        protected override void Close(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext)
        {
            var controllers = NavigationController.ViewControllers;
            if (controllers == null)
                return;

            var animated = navigationContext.GetMetadataOrDefault().Get(NavigationMetadata.Animated, Animated);
            if (controllers.Length != 1 && Equals(NavigationController.TopViewController, view))
            {
                NavigationController.PopViewController(animated);
                return;
            }

            var index = Array.IndexOf(controllers, view);
            if (index < 0)
                return;

            var cancelableRequest = new CancelableRequest(state: this);
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, cancelableRequest, navigationContext.GetMetadataOrDefault());
            if (cancelableRequest.Cancel.GetValueOrDefault())
                return;

            Array.Copy(controllers, index + 1, controllers, index, controllers.Length - index - 1);
            Array.Resize(ref controllers, controllers.Length - 1);
            NavigationController.SetViewControllers(controllers, animated);
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed, null, navigationContext.GetMetadataOrDefault());
        }

        private void ShowInternal(bool bringToFront, IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext)
        {
            var metadata = navigationContext.GetMetadataOrDefault();
            if (metadata.Get(NavigationMetadata.ClearBackStack))
            {
                var task = NavigationDispatcher.ClearBackStackAsync(NavigationType, mediator.ViewModel, false, metadata, Presenter);
                if (!task.IsCompleted)
                {
                    task.ContinueWithEx((this, bringToFront, mediator, view, navigationContext), (_, state) => state.Item1.ShowInternal(state.bringToFront, state.mediator, state.view, state.navigationContext));
                    return;
                }
            }

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