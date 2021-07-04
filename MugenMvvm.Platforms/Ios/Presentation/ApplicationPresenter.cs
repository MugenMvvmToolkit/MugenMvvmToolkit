using System;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Ios.Constants;
using MugenMvvm.Ios.Views;
using MugenMvvm.Navigation;
using UIKit;

namespace MugenMvvm.Ios.Presentation
{
    public sealed class ApplicationPresenter : IPresenterComponent, IHasPriority
    {
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly Type _rootViewModelType;
        private readonly IViewManager? _viewManager;
        private readonly IViewModelManager? _viewModelManager;
        private readonly bool _wrapToNavigationController;

        public ApplicationPresenter(Type rootViewModelType, bool wrapToNavigationController, IViewModelManager? viewModelManager = null,
            IViewManager? viewManager = null, INavigationDispatcher? navigationDispatcher = null)
        {
            Should.NotBeNull(rootViewModelType, nameof(rootViewModelType));
            _rootViewModelType = rootViewModelType;
            _wrapToNavigationController = wrapToNavigationController;
            _viewModelManager = viewModelManager;
            _viewManager = viewManager;
            _navigationDispatcher = navigationDispatcher;
        }

        public int Priority { get; init; } = ComponentPriority.Max;

        private static void SetNavigationController(IPresenter presenter, UIWindow window, UINavigationController controller)
        {
            presenter.AddComponent(new NavigationControllerViewPresenterMediator(controller) {Priority = ComponentPriority.Min});
            window.RootViewController = controller;
            window.MakeKeyAndVisible();
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var isAppRequest = request is UIApplication;
            if (isAppRequest)
                presenter.RemoveComponent(this);
            var application = UIApplication.SharedApplication;
            var window = application.Delegate.GetWindow();
            if (window == null)
            {
                window = new UIWindow(UIScreen.MainScreen.Bounds);
                application.Delegate.SetWindow(window);
            }

            if (window.RootViewController != null)
            {
                //restored but without main view model
                if (isAppRequest && _wrapToNavigationController && window.RootViewController is UINavigationController navController && navController.TopViewController == null)
                    return presenter.Show(_viewModelManager.DefaultIfNull().GetViewModel(_rootViewModelType, metadata));
                return default;
            }

            var viewModel = MugenExtensions.TryGetViewModelView(request, out UIViewController? view);
            if (view == null && viewModel == null)
            {
                viewModel = _viewModelManager.DefaultIfNull().GetViewModel(_rootViewModelType, metadata);
                if (_wrapToNavigationController)
                {
                    SetNavigationController(presenter, window, new MugenNavigationController());
                    return presenter.Show(viewModel, cancellationToken, metadata);
                }

                SetRootController(window, viewModel, (UIViewController) viewModel.GetOrCreateView(metadata, _viewManager).Target, metadata);
                return default;
            }

            //restored root view
            if (viewModel != null && view != null)
            {
                SetRootController(window, viewModel, view, metadata);
                return default;
            }

            //restored navigation controller
            if (_wrapToNavigationController && view is UINavigationController navigationController)
            {
                SetNavigationController(presenter, window, navigationController);
                return default;
            }

            return default;
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            default;

        private void SetRootController(UIWindow window, IViewModelBase viewModel, UIViewController view, IReadOnlyMetadataContext? metadata)
        {
            var navigationDispatcher = _navigationDispatcher.DefaultIfNull();
            var context = navigationDispatcher.GetNavigationContext(viewModel, NavigationProvider.System, IosInternalConstants.RootNavigationId, NavigationType.Window,
                NavigationMode.New, metadata);
            navigationDispatcher.OnNavigating(context);
            window.RootViewController = view;
            window.MakeKeyAndVisible();
            navigationDispatcher.OnNavigated(context);
        }
    }
}