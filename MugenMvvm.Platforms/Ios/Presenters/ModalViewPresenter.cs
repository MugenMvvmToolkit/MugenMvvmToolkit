﻿using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Ios.Interfaces;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;
using MugenMvvm.Requests;
using UIKit;

namespace MugenMvvm.Ios.Presenters
{
    public class ModalViewPresenter : ViewPresenterBase<UIViewController>
    {
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly MugenAdaptivePresentationControllerDelegate _presentationControllerDelegate;
        private readonly IViewManager? _viewManager;

        public ModalViewPresenter(IViewManager? viewManager = null, INavigationDispatcher? navigationDispatcher = null)
        {
            _viewManager = viewManager;
            _navigationDispatcher = navigationDispatcher;
            _presentationControllerDelegate = new MugenAdaptivePresentationControllerDelegate(this);
        }

        public override NavigationType NavigationType => NavigationType.Popup;

        public bool Animated { get; set; } = true;

        public bool NonModal { get; set; } = true;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected override bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata)
            => base.CanPresent(presenter, viewModel, mapping, metadata) && typeof(IModalView).IsAssignableFrom(mapping.ViewType);

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext) => Default.CompletedTask;

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                view.ModalInPresentation = !navigationContext.GetOrDefault(NavigationMetadata.NonModal, NonModal);
                view.PresentationController.Delegate = _presentationControllerDelegate;
            }

            if (navigationContext.NavigationMode != NavigationMode.New)
                return Default.CompletedTask;

            var topView = NavigationDispatcher.GetTopView<UIViewController>(includePending: false, metadata: navigationContext.GetMetadataOrDefault());
            if (topView == null)
                ExceptionManager.ThrowObjectNotInitialized(typeof(UIViewController), nameof(topView));
            topView.PresentViewController(view, navigationContext.GetOrDefault(NavigationMetadata.Animated, Animated), null);
            return Default.CompletedTask;
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext)
        {
            var request = new CancelableRequest();
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, request, navigationContext.GetMetadataOrDefault());
            if (!request.Cancel.GetValueOrDefault())
            {
                var animated = navigationContext.GetOrDefault(NavigationMetadata.Animated, Animated);
                var childViewController = view.PresentedViewController;
                view.DismissViewController(animated, () =>
                {
                    ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed, null, navigationContext.GetMetadataOrDefault());
                    if (childViewController != null)
                        ViewManager.OnLifecycleChanged(childViewController, ViewLifecycleState.Closed, null, navigationContext.GetMetadataOrDefault());
                });
            }

            return Default.CompletedTask;
        }

        private sealed class MugenAdaptivePresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
        {
            private readonly ModalViewPresenter _presenter;

            public MugenAdaptivePresentationControllerDelegate(ModalViewPresenter presenter)
            {
                _presenter = presenter;
            }

            public override bool ShouldDismiss(UIPresentationController presentationController)
            {
                var request = new CancelableRequest();
                _presenter.ViewManager.OnLifecycleChanged(presentationController.PresentedViewController, ViewLifecycleState.Closing, request);
                return !request.Cancel.GetValueOrDefault();
            }

            public override void DidDismiss(UIPresentationController presentationController) =>
                _presenter.ViewManager.OnLifecycleChanged(presentationController.PresentedViewController, ViewLifecycleState.Closed);
        }
    }
}