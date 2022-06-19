﻿using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Ios.Interfaces;
using MugenMvvm.Metadata;
using MugenMvvm.Presentation;
using MugenMvvm.Requests;
using UIKit;

namespace MugenMvvm.Ios.Presentation
{
    public class ModalViewPresenterMediator : ViewPresenterMediatorBase<UIViewController>
    {
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly MugenAdaptivePresentationControllerDelegate _presentationControllerDelegate;
        private readonly IViewManager? _viewManager;

        public ModalViewPresenterMediator(IThreadDispatcher? threadDispatcher = null, IViewManager? viewManager = null, INavigationDispatcher? navigationDispatcher = null)
            : base(threadDispatcher)
        {
            _viewManager = viewManager;
            _navigationDispatcher = navigationDispatcher;
            _presentationControllerDelegate = new MugenAdaptivePresentationControllerDelegate(this);
        }

        public override NavigationType NavigationType => NavigationType.Popup;

        protected override bool IsActivateSupported => false;

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected override bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata)
            => base.CanPresent(presenter, viewModel, mapping, metadata) && typeof(IModalView).IsAssignableFrom(mapping.ViewType);

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext,
            CancellationToken cancellationToken) => Task.CompletedTask;

        protected override async Task ShowAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                view.ModalInPresentation = navigationContext.GetOrDefault(NavigationMetadata.Modal);
                view.PresentationController.Delegate = _presentationControllerDelegate;
            }

            if (navigationContext.NavigationMode != NavigationMode.New)
                return;

            var topView = await NavigationDispatcher.GetTopViewAsync<UIViewController>(null, false, mediator.ViewModel, navigationContext.GetMetadataOrDefault());
            topView.PresentViewController(view, navigationContext.GetOrDefault(NavigationMetadata.Animated), null);
        }

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, UIViewController view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var request = new CancelableRequest();
            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, request, navigationContext.GetMetadataOrDefault());
            if (!request.Cancel.GetValueOrDefault())
            {
                var animated = navigationContext.GetOrDefault(NavigationMetadata.Animated);
                var childViewController = view.PresentedViewController;
                view.DismissViewController(animated, () =>
                {
                    ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed, null, navigationContext.GetMetadataOrDefault());
                    if (childViewController != null)
                        ViewManager.OnLifecycleChanged(childViewController, ViewLifecycleState.Closed, null, navigationContext.GetMetadataOrDefault());
                });
            }

            return Task.CompletedTask;
        }

        private sealed class MugenAdaptivePresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
        {
            private readonly ModalViewPresenterMediator _presenter;

            public MugenAdaptivePresentationControllerDelegate(ModalViewPresenterMediator presenter)
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