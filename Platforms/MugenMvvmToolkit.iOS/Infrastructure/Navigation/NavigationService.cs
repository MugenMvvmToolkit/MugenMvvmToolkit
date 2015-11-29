#region Copyright

// ****************************************************************************
// <copyright file="NavigationService.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Interfaces.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.iOS.Models.EventArg;
using MugenMvvmToolkit.iOS.Views;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private UIWindow _window;

        #endregion

        #region Constructors

        public NavigationService([NotNull] UIWindow window)
        {
            Should.NotBeNull(window, "window");
            _window = window;
            if (_window.RootViewController == null)
            {
                NSObject observer = null;
                observer = UIWindow.Notifications.ObserveDidBecomeVisible((sender, args) =>
                {
                    EnsureInitialized();
                    observer.Dispose();
                });
            }
            UseAnimations = true;
        }

        public NavigationService([NotNull] UINavigationController navigationController)
        {
            Should.NotBeNull(navigationController, "navigationController");
            InitializeNavigationController(navigationController);
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        protected UINavigationController NavigationController { get; private set; }

        #endregion

        #region Implementation of INavigationService

        public virtual bool CanGoBack
        {
            get
            {
                EnsureInitialized();
                return NavigationController != null && NavigationController.ViewControllers != null &&
                       NavigationController.ViewControllers.Length > 0;
            }
        }

        public virtual bool CanGoForward
        {
            get { return false; }
        }

        public virtual object CurrentContent
        {
            get
            {
                EnsureInitialized();
                if (NavigationController == null)
                    return null;
                return NavigationController.TopViewController;
            }
        }

        public virtual void GoBack()
        {
            EnsureInitialized();
            GoBackInternal();
        }

        public virtual void GoForward()
        {
            Should.MethodBeSupported(false, "GoForward()");
        }

        public virtual string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            EnsureInitialized();
            var cancelArgs = args as NavigatingCancelEventArgs;
            if (cancelArgs == null)
            {
                var eventArgs = args as NavigationEventArgs;
                if (eventArgs == null)
                    return null;
                return eventArgs.Parameter;
            }
            return cancelArgs.Parameter;
        }

        public virtual bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, "args");
            if (!args.IsCancelable)
                return false;
            EnsureInitialized();
            var eventArgs = ((NavigatingCancelEventArgs)args);
            if (eventArgs.NavigationMode == NavigationMode.Back)
                return GoBackInternal();
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, dataContext);
        }

        public virtual bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            EnsureInitialized();
            if (dataContext == null)
                dataContext = DataContext.Empty;
            bool bringToFront;
            dataContext.TryGetData(NavigationProviderConstants.BringToFront, out bringToFront);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter)))
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
                    viewController = (UIViewController)ServiceProvider.Get<IViewManager>().GetViewAsync(source, dataContext).Result;
                else
                    viewController = (UIViewController)ViewManager.GetOrCreateView(viewModel, null, dataContext);
            }
            viewController.SetNavigationParameter(parameter);
            bool shouldNavigate = true;
            if (_window != null)
            {
                var controller = _window.RootViewController as UINavigationController;
                if (controller == null)
                {
                    shouldNavigate = false;
                    controller = new MvvmNavigationController(viewController);
                    _window.RootViewController = controller;
                }
                InitializeNavigationController(controller);
            }
            if (shouldNavigate)
            {
                bool animated;
                if (dataContext.TryGetData(NavigationConstants.UseAnimations, out animated))
                {
                    if (viewModel != null)
                        viewModel.Settings.State.AddOrUpdate(NavigationConstants.UseAnimations, animated);
                }
                else
                    animated = UseAnimations;
                if (!ClearNavigationStackIfNeed(viewController, dataContext, animated))
                    NavigationController.PushViewController(viewController, animated);
            }
            var view = viewController as IViewControllerView;
            if (view == null || view.Mediator.IsAppeared)
                RaiseNavigated(viewController, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter);
            else
            {
                if (bringToFront)
                    view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerRefresh;
                else
                    view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerNew;
            }
            return true;
        }

        public virtual bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (NavigationController == null)
                return false;
            var controllers = NavigationController.ViewControllers;
            if (controllers == null)
                return false;
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].DataContext() == viewModel)
                    return true;
            }
            return false;
        }

        public virtual bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (dataContext == null)
                dataContext = DataContext.Empty;
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
                var topViewController = NavigationController.TopViewController;
                NavigationController.SetViewControllers(controllers.ToArray(), animated);
                if (!ReferenceEquals(NavigationController.TopViewController, topViewController))
                    DidPopViewController(this, EventArgs.Empty);
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
            EventHandler<INavigationService, NavigationEventArgsBase> handler = Navigated;
            if (handler != null)
                handler(this, args);
        }

        protected void EnsureInitialized()
        {
            if (_window == null)
                return;
            var rootViewController = _window.RootViewController as UINavigationController;
            if (NavigationController == null && rootViewController != null)
                InitializeNavigationController(rootViewController);
        }

        private void InitializeNavigationController(UINavigationController navigationController)
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
            var currentContent = CurrentContent;
            if (currentContent == null)
                return;
            var dataContext = currentContent.DataContext() as IEventPublisher;
            if (dataContext != null)
                dataContext.Publish(this, StateChangedMessage.Empty);
        }

        private bool GoBackInternal()
        {
            Should.BeSupported(CanGoBack, "Go back is not supported in current state.");
            bool animated;
            var viewModel = CurrentContent == null ? null : CurrentContent.DataContext() as IViewModel;
            if (viewModel == null || !viewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out animated))
                animated = UseAnimations;
            return NavigationController.PopViewController(animated) != null;
        }

        private void ShouldPopViewController(object sender, CancelEventArgs args)
        {
            string parameter = null;
            var controllers = NavigationController.ViewControllers;
            if (controllers.Length > 1)
                parameter = controllers[controllers.Length - 2].GetNavigationParameter() as string;
            args.Cancel = !RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Back, parameter));
        }

        private void DidPopViewController(object sender, EventArgs eventArgs)
        {
            var controller = NavigationController.TopViewController;
            var view = controller as IViewControllerView;
            if (view == null || view.Mediator.IsAppeared)
                RaiseNavigated(controller, NavigationMode.Back, controller.GetNavigationParameter() as string);
            else
                view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandlerBack;
        }

        private void RaiseNavigated(object content, NavigationMode mode, string parameter)
        {
            if (Navigated != null)
                RaiseNavigated(new NavigationEventArgs(content, parameter, mode));
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
            RaiseNavigated(sender, NavigationMode.Back, sender.GetNavigationParameter() as string);
        }

        private void OnViewDidAppearHandlerNew(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerNew;
            RaiseNavigated(sender, NavigationMode.New, sender.GetNavigationParameter() as string);
        }

        private void OnViewDidAppearHandlerRefresh(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandlerNew;
            RaiseNavigated(sender, NavigationMode.Refresh, sender.GetNavigationParameter() as string);
        }

        #endregion
    }
}
