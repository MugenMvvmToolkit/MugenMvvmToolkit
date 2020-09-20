using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Ios.Constants;
using MugenMvvm.Ios.Views;
using MugenMvvm.Navigation;
using UIKit;

namespace MugenMvvm.Ios.Presenters
{
    public sealed class ApplicationPresenter : IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly Type _rootViewModelType;
        private readonly IViewManager? _viewManager;
        private readonly IViewModelManager? _viewModelManager;
        private readonly bool _wrapToNavigationController;

        #endregion

        #region Constructors

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

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Max;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
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

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => default;

        #endregion

        #region Methods

        private void SetRootController(UIWindow window, IViewModelBase viewModel, UIViewController view, IReadOnlyMetadataContext? metadata)
        {
            var navigationDispatcher = _navigationDispatcher.DefaultIfNull();
            var context = new NavigationContext(viewModel, Default.NavigationProvider, IosInternalConstants.RootNavigationId, NavigationType.Window, NavigationMode.New, metadata);
            navigationDispatcher.OnNavigating(context);
            window.RootViewController = view;
            window.MakeKeyAndVisible();
            navigationDispatcher.OnNavigated(context);
        }

        private static void SetNavigationController(IPresenter presenter, UIWindow window, UINavigationController controller)
        {
            presenter.AddComponent(new NavigationControllerViewPresenter(controller) {Priority = ComponentPriority.Min});
            window.RootViewController = controller;
            window.MakeKeyAndVisible();
        }

        #endregion
    }
}