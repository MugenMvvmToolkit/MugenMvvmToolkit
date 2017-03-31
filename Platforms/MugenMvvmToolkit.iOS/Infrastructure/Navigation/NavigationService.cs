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

        private UIViewController CurrentContent => NavigationController?.TopViewController;

        #endregion

        #region Implementation of INavigationService

        object INavigationService.CurrentContent => CurrentContent;

        public string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            var cancelArgs = args as NavigatingCancelEventArgs;
            if (cancelArgs == null)
                return (args as NavigationEventArgs)?.Parameter;
            return cancelArgs.Parameter;
        }

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (!args.IsCancelable)
                return false;
            if (args.NavigationMode == NavigationMode.Remove && args.Context != null)
                return TryClose(args.Context);
            var eventArgs = (NavigatingCancelEventArgs)args;
            if (eventArgs.NavigationMode == NavigationMode.Back)
                return GoBack(args.Context);
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, args.Context);
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
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
                        NavigationController.SetViewControllers(viewControllers.ToArray(), false);
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

            var view = viewController as IViewControllerView;
            if (view != null)
            {
                viewController.SetNavigationContext(dataContext, false);
                if (bringToFront)
                    view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerRefresh;
                else
                    view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerNew;
            }

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

            if (view == null)
                RaiseNavigated(viewController, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter, dataContext);
            return true;
        }

        public bool CanClose(IDataContext dataContext)
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

        public bool TryClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (CurrentContent?.DataContext() == viewModel)
                return GoBack(dataContext);

            if (!CanClose(dataContext))
                return false;
            if (RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Remove, null, dataContext)))
            {
                bool closed = false;
                var controllers = NavigationController.ViewControllers.ToList();
                for (int i = 0; i < controllers.Count; i++)
                {
                    if (controllers[i].DataContext() == viewModel)
                    {
                        controllers.RemoveAt(i);
                        --i;
                        closed = true;
                    }
                }
                if (NavigationController.ViewControllers.Length != controllers.Count)
                {
                    NavigationController.SetViewControllers(controllers.ToArray(), IsAnimated(dataContext, viewModel));
                    RaiseNavigated(new NavigationEventArgs(viewModel, null, NavigationMode.Remove, dataContext));
                }
                return closed;
            }
            return true;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

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
                ex.ShouldPopViewController += ShouldPopViewController;
            _window = null;
            _getOrCreateController = null;
            _restoreNavigationController = null;
            (CurrentContent?.DataContext() as IViewModel)?.InvalidateCommands();
        }

        private bool GoBack(IDataContext context)
        {
            var controllers = NavigationController.ViewControllers;
            if (controllers == null || controllers.Length == 0)
                return false;

            var isAnimated = IsAnimated(context, CurrentContent?.DataContext() as IViewModel);
            if (controllers.Length == 1)
            {
                var controller = controllers[0];
                if (RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Back, null, context)))
                {
                    var view = controller as IViewControllerView;
                    if (view == null || view.Mediator.IsDisappeared)
                    {
                        NavigationController.SetViewControllers(Empty.Array<UIViewController>(), false);
                        RaiseNavigated(null, NavigationMode.Back, null, context);
                    }
                    else
                    {
                        controller.SetNavigationContext(context, true);
                        view.Mediator.ViewDidDisappearHandler += OnViewDidDisappearHandlerBack;
                        NavigationController.SetViewControllers(Empty.Array<UIViewController>(), isAnimated);
                    }
                }
            }
            else
            {
                CurrentContent?.SetNavigationContext(context, true);
                NavigationController.PopViewController(isAnimated);
            }
            return true;
        }

        private void ShouldPopViewController(object sender, CancelEventArgs args)
        {
            var controllers = NavigationController.ViewControllers ?? Empty.Array<UIViewController>();
            UIViewController prevController = null, currentController = null;
            if (controllers.Length > 0)
                currentController = controllers[controllers.Length - 1];
            if (controllers.Length > 1)
                prevController = controllers[controllers.Length - 2];

            //new navigation is not completed, skip back navigation
            var newNavigationContext = currentController?.GetNavigationContext(false, false);
            if (newNavigationContext != null && newNavigationContext.Count != 0)
            {
                args.Cancel = true;
                return;
            }

            args.Cancel = !RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Back, prevController.GetNavigationParameter(), currentController.GetNavigationContext(true, false)));
            if (args.Cancel)
                return;

            var viewControllerView = currentController as IViewControllerView;
            if (viewControllerView == null)
                RaiseNavigated(prevController, NavigationMode.Back, prevController.GetNavigationParameter(), currentController.GetNavigationContext(true, true));
            else
            {
                EventHandler<UIViewController, ValueEventArgs<bool>> handler = OnViewDidDisappearHandlerBack;
                viewControllerView.Mediator.ViewDidDisappearHandler -= handler;
                viewControllerView.Mediator.ViewDidDisappearHandler += handler;
            }
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
                var controllers = NavigationController.ViewControllers;
                if (controllers != null)
                {
                    Array.Reverse(controllers);
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        var controller = controllers[i];
                        if (ReferenceEquals(controller, newItem))
                            continue;
                        var viewModel = controller.DataContext() as IViewModel;
                        if (viewModel != null)
                        {
                            var ctx = new DataContext(context);
                            ctx.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
                            RaiseNavigated(controller, NavigationMode.Remove, null, ctx);
                        }
                    }
                }
                NavigationController.SetViewControllers(new[] { newItem }, animated);
                return true;
            }
            return false;
        }

        private void OnViewDidDisappearHandlerBack(UIViewController sender, ValueEventArgs<bool> args)
        {
            var content = ReferenceEquals(sender, CurrentContent)
                ? NavigationController.ViewControllers?.LastOrDefault(controller => !ReferenceEquals(controller, sender))
                : CurrentContent;
            ((IViewControllerView)sender).Mediator.ViewDidDisappearHandler -= OnViewDidDisappearHandlerBack;
            RaiseNavigated(content, NavigationMode.Back, content.GetNavigationParameter(), sender.GetNavigationContext(true, true));
        }

        private void OnViewDidAppearHandlerNew(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerNew;
            RaiseNavigated(sender, NavigationMode.New, sender.GetNavigationParameter(), sender.GetNavigationContext(false, true));
        }

        private void OnViewDidAppearHandlerRefresh(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerRefresh;
            RaiseNavigated(sender, NavigationMode.Refresh, sender.GetNavigationParameter(), sender.GetNavigationContext(false, true));
        }

        private bool IsAnimated(IDataContext context, IViewModel viewModel)
        {
            bool result;
            if (context != null && context.TryGetData(NavigationConstants.UseAnimations, out result))
                return result;
            if (viewModel != null && viewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out result))
                return result;
            return UseAnimations;
        }

        #endregion
    }
}
