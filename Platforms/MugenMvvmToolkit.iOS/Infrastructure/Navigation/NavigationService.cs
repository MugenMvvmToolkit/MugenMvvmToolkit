#region Copyright

// ****************************************************************************
// <copyright file="NavigationService.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Interfaces.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.iOS.Models.EventArg;
using MugenMvvmToolkit.iOS.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private UIWindow _window;
        private Func<UIWindow, UIViewController, UINavigationController> _getOrCreateController;
        private Func<UIWindow, UINavigationController> _restoreNavigationController;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationService([NotNull] UIWindow window,
            Func<UIWindow, UIViewController, UINavigationController> getOrCreateController = null,
            Func<UIWindow, UINavigationController> restoreNavigationController = null)
        {
            Should.NotBeNull(window, nameof(window));
            _window = window;
            _getOrCreateController = getOrCreateController;
            _restoreNavigationController = restoreNavigationController;
            if (_window.RootViewController == null)
            {
                NSObject observer = null;
                observer = UIWindow.Notifications.ObserveDidBecomeVisible((sender, args) =>
                {
                    var uiWindow = _window;
                    if (uiWindow != null)
                        InitializeNavigationController(RestoreNavigationController(uiWindow));
                    observer.Dispose();
                });
            }
            UseAnimations = true;
        }

        public NavigationService([NotNull] UINavigationController navigationController)
        {
            Should.NotBeNull(navigationController, nameof(navigationController));
            InitializeNavigationController(navigationController);
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        protected UINavigationController NavigationController { get; private set; }

        #endregion

        #region Implementation of INavigationService

        public virtual object CurrentContent => NavigationController?.TopViewController;

        public virtual string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            var cancelArgs = args as NavigatingCancelEventArgs;
            if (cancelArgs == null)
                return (args as NavigationEventArgs)?.Parameter;
            return cancelArgs.Parameter;
        }

        public virtual bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (!args.IsCancelable)
                return false;
            if (args.NavigationMode == NavigationMode.Remove && args.Context != null)
                return TryClose(args.Context);
            var eventArgs = (NavigatingCancelEventArgs)args;
            if (eventArgs.NavigationMode == NavigationMode.Back)
                return GoBackInternal();
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, args.Context);
        }

        public virtual bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            if (dataContext == null)
                dataContext = DataContext.Empty;
            bool bringToFront;
            dataContext.TryGetData(NavigationProviderConstants.BringToFront, out bringToFront);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter, dataContext)))
                return false;

            UIViewController viewController = null;
            IViewModel viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (bringToFront && viewModel != null)
            {
                var viewControllers = new List<UIViewController>(NavigationController.ViewControllers);
                for (int i = 0; i < viewControllers.Count; i++)
                {
                    var controller = viewControllers[i];
                    if (controller.DataContext() == viewModel)
                    {
                        viewControllers.RemoveAt(i);
                        viewController = controller;
                        NavigationController.ViewControllers = viewControllers.ToArray();
                        dataContext.AddOrUpdate(NavigationProviderConstants.InvalidateCache, true);
                        break;
                    }
                }
            }

            if (viewController == null)
            {
                if (viewModel == null)
                    viewController = (UIViewController)ServiceProvider.ViewManager.GetViewAsync(source, dataContext).Result;
                else
                    viewController = (UIViewController)ServiceProvider.ViewManager.GetOrCreateView(viewModel, null, dataContext);
            }
            viewController.SetNavigationParameter(parameter);
            bool shouldNavigate = true;
            if (_window != null)
            {
                bool navigated;
                InitializeNavigationController(GetNavigationController(_window, viewController, out navigated));
                shouldNavigate = !navigated;
                _window = null;
            }
            if (shouldNavigate)
            {
                bool animated;
                if (dataContext.TryGetData(NavigationConstants.UseAnimations, out animated))
                    viewModel?.Settings.State.AddOrUpdate(NavigationConstants.UseAnimations, animated);
                else
                    animated = UseAnimations;
                if (!ClearNavigationStackIfNeed(viewController, dataContext, animated))
                    NavigationController.PushViewController(viewController, animated);
            }
            var view = viewController as IViewControllerView;
            if (view == null || view.Mediator.IsAppeared)
                RaiseNavigated(viewController, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter, dataContext);
            else
            {
                viewController.SetNavigationContext(dataContext);
                if (bringToFront)
                    view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerRefresh;
                else
                    view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerNew;
            }
            return true;
        }

        public virtual bool CanClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            var controllers = NavigationController?.ViewControllers;
            if (controllers == null)
                return false;
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].DataContext() == viewModel)
                    return true;
            }
            return false;
        }

        public virtual bool TryClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (CurrentContent?.DataContext() == viewModel)
            {
                (CurrentContent as UIViewController)?.SetNavigationContext(dataContext);
                return GoBackInternal();
            }

            if (!CanClose(dataContext) || !RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Remove, null, dataContext)))
                return false;
            bool animated;
            if (!dataContext.TryGetData(NavigationConstants.UseAnimations, out animated))
                animated = UseAnimations;
            var controllers = NavigationController.ViewControllers.ToList();
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].DataContext() == viewModel)
                {
                    controllers.RemoveAt(i);
                    --i;
                }
            }
            if (NavigationController.ViewControllers.Length != controllers.Count)
            {
                NavigationController.SetViewControllers(controllers.ToArray(), animated);
                RaiseNavigated(new NavigationEventArgs(viewModel, null, NavigationMode.Remove, dataContext));
            }
            return true;
        }

        public virtual event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public virtual event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        protected virtual bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler == null)
                return true;
            handler(this, args);
            return !args.Cancel;
        }

        protected virtual void RaiseNavigated(NavigationEventArgs args)
        {
            Navigated?.Invoke(this, args);
        }

        protected virtual UINavigationController RestoreNavigationController(UIWindow window)
        {
            if (_restoreNavigationController == null)
                return _window.RootViewController as UINavigationController;
            return _restoreNavigationController(window);
        }

        protected virtual UINavigationController GetNavigationController(UIWindow window, UIViewController rootController, out bool isRootNavigated)
        {
            isRootNavigated = true;
            if (_getOrCreateController == null)
            {
                var controller = window.RootViewController as UINavigationController;
                if (controller == null)
                {
                    controller = new MvvmNavigationController(rootController);
                    window.RootViewController = controller;
                    return controller;
                }
                isRootNavigated = false;
                return controller;
            }
            return _getOrCreateController(window, rootController);
        }

        protected void InitializeNavigationController(UINavigationController navigationController)
        {
            if (NavigationController != null)
                return;
            NavigationController = navigationController;
            var ex = navigationController as IMvvmNavigationController;
            if (ex != null)
            {
                ex.ShouldPopViewController += ShouldPopViewController;
                ex.DidPopViewController += DidPopViewController;
            }
            _window = null;
            _getOrCreateController = null;
            _restoreNavigationController = null;
            (CurrentContent?.DataContext() as IViewModel)?.InvalidateCommands();
        }

        private bool GoBackInternal()
        {
            bool animated;
            var viewModel = CurrentContent?.DataContext() as IViewModel;
            if (viewModel == null || !viewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out animated))
                animated = UseAnimations;
            return NavigationController.PopViewController(animated) != null;
        }

        private void ShouldPopViewController(object sender, CancelEventArgs args)
        {
            string parameter = null;
            IDataContext context = null;
            UIViewController oldController = null;
            var controllers = NavigationController.ViewControllers;
            if (controllers.Length > 1)
            {
                var controller = controllers[controllers.Length - 2];
                oldController = controllers[controllers.Length - 1];
                parameter = controller.GetNavigationParameter() as string;
                context = oldController.GetNavigationContext(false);
                controller.SetNavigationContext(context);
            }
            args.Cancel = !RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Back, parameter, context));
            if (!args.Cancel)
                oldController?.GetNavigationContext(true);
        }

        private void DidPopViewController(object sender, EventArgs eventArgs)
        {
            var controller = NavigationController.TopViewController;
            var view = controller as IViewControllerView;
            if (view == null || view.Mediator.IsAppeared)
                RaiseNavigated(controller, NavigationMode.Back, controller.GetNavigationParameter() as string, controller.GetNavigationContext(true));
            else
                view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerBack;
        }

        private void RaiseNavigated(object content, NavigationMode mode, string parameter, IDataContext context)
        {
            if (Navigated != null)
                RaiseNavigated(new NavigationEventArgs(content, parameter, mode, context));
        }

        private bool ClearNavigationStackIfNeed(UIViewController newItem, IDataContext context, bool animated)
        {
            if (context == null)
                context = DataContext.Empty;
            if (context.GetData(NavigationConstants.ClearBackStack) && NavigationController != null)
            {
                NavigationController.SetViewControllers(new[] { newItem }, animated);
                context.AddOrUpdate(NavigationProviderConstants.InvalidateAllCache, true);
                return true;
            }
            return false;
        }

        private void OnViewDidAppearHandlerBack(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerBack;
            RaiseNavigated(sender, NavigationMode.Back, sender.GetNavigationParameter() as string, sender.GetNavigationContext(true));
        }

        private void OnViewDidAppearHandlerNew(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerNew;
            RaiseNavigated(sender, NavigationMode.New, sender.GetNavigationParameter() as string, sender.GetNavigationContext(true));
        }

        private void OnViewDidAppearHandlerRefresh(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerRefresh;
            RaiseNavigated(sender, NavigationMode.Refresh, sender.GetNavigationParameter() as string, sender.GetNavigationContext(true));
        }

        #endregion
    }
}
