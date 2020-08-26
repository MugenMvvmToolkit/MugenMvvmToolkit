using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Ios.Views;
using MugenMvvm.Navigation;
using UIKit;

namespace MugenMvvm.Ios.Presenters
{
    public sealed class ApplicationPresenter : ComponentDecoratorBase<IPresenter, IPresenterComponent>, IPresenterComponent, IHasPriority
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
            if (!(request is UIApplication application))
                return Components.TryShow(presenter, request, cancellationToken, metadata);
            presenter.RemoveComponent(this);

            var window = application.Delegate.GetWindow();
            if (window == null)
            {
                window = new UIWindow(UIScreen.MainScreen.Bounds);
                application.Delegate.SetWindow(window);
            }
            if (window.RootViewController != null)
                return default;

            var viewModel = _viewModelManager.DefaultIfNull().GetViewModel(_rootViewModelType, metadata);
            if (_wrapToNavigationController)
            {
                var controller = new MugenNavigationController();
                presenter.AddComponent(new NavigationControllerViewPresenter(controller) { Priority = ComponentPriority.Min });
                window.RootViewController = controller;
                window.MakeKeyAndVisible();
                return presenter.Show(viewModel, cancellationToken, metadata);
            }

            var view = viewModel.GetOrCreateView(metadata, _viewManager);
            var context = new NavigationContext(viewModel, Default.NavigationProvider, "root", NavigationType.Window, NavigationMode.New, metadata);
            _navigationDispatcher.DefaultIfNull().OnNavigating(context);
            window.RootViewController = (UIViewController)view.Target;
            window.MakeKeyAndVisible();
            _navigationDispatcher.DefaultIfNull().OnNavigated(context);
            return default;
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => Components.TryClose(presenter, request, cancellationToken, metadata);

        #endregion
    }
}