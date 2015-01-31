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
using System.ComponentModel;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Views;
using UIKit;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private UIWindow _window;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        public NavigationService([NotNull] UINavigationController navigationController)
        {
            Should.NotBeNull(navigationController, "navigationController");
            InitializeNavigationController(navigationController);
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        /// <summary>
        ///     Gets the current <see cref="MvvmNavigationController" />.
        /// </summary>
        protected UINavigationController NavigationController { get; private set; }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public virtual bool CanGoBack
        {
            get
            {
                EnsureInitialized();
                return NavigationController != null && NavigationController.ViewControllers != null &&
                       NavigationController.ViewControllers.Length > 0;
            }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public virtual bool CanGoForward
        {
            get { return false; }
        }

        /// <summary>
        ///     The current content.
        /// </summary>
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

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public virtual void GoBack()
        {
            EnsureInitialized();
            GoBackInternal();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public virtual void GoForward()
        {
            Should.MethodBeSupported(false, "GoForward()");
        }

        /// <summary>
        ///     Gets a navigation parameter from event args.
        /// </summary>
        public virtual object GetParameterFromArgs(EventArgs args)
        {
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

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        public virtual bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            EnsureInitialized();
            if (!args.IsCancelable)
                return false;
            var eventArgs = ((NavigatingCancelEventArgs)args);
            if (eventArgs.NavigationMode == NavigationMode.Back)
                return GoBackInternal();
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, dataContext);
        }

        /// <summary>
        ///     Displays the content located at the specified <see cref="IViewMappingItem" />.
        /// </summary>
        /// <param name="source">
        ///     The <c>IViewPageMappingItem</c> of the content to display.
        /// </param>
        /// <param name="parameter">
        ///     A <see cref="T:System.Object" /> that contains data to be used for processing during
        ///     navigation.
        /// </param>
        /// <param name="dataContext">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the content was successfully displayed; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Navigate(IViewMappingItem source, object parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            EnsureInitialized();
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, NavigationMode.New, parameter)))
                return false;
            if (dataContext == null)
                dataContext = DataContext.Empty;

            IViewModel viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            UIViewController viewController;
            if (viewModel == null)
                viewController = (UIViewController)ServiceProvider.IocContainer.Get(source.ViewType);
            else
                viewController = (UIViewController)ViewManager.GetOrCreateView(viewModel, null, dataContext);

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
                if (!dataContext.TryGetData(NavigationConstants.UseAnimations, out animated))
                    animated = UseAnimations;
                NavigationController.PushViewController(viewController, animated);
                ClearNavigationStackIfNeed(viewController, dataContext);
            }
            var view = viewController as IViewControllerView;
            if (view == null || view.Mediator.IsAppeared)
                RaiseNavigated(viewController, NavigationMode.New, parameter);
            else
                view.Mediator.ViewDidAppearHandler += OnViewDidAppearHandler;
            return true;
        }

        /// <summary>
        ///     Raised prior to navigation.
        /// </summary>
        public virtual event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        /// <summary>
        ///     Raised after navigation.
        /// </summary>
        public virtual event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        /// <summary>
        ///     Invokes the <see cref="Navigating" /> event.
        /// </summary>
        protected virtual bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler == null)
                return true;
            handler(this, args);
            return !args.Cancel;
        }

        /// <summary>
        ///     Invokes the <see cref="Navigated" /> event.
        /// </summary>
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
            var dataContext = ViewManager.GetDataContext(currentContent) as IEventPublisher;
            if (dataContext != null)
                dataContext.Publish(this, StateChangedMessage.Empty);
        }

        private bool GoBackInternal()
        {
            Should.BeSupported(CanGoBack, "Go back is not supported in current state.");
            return NavigationController.PopViewController(true) != null;
        }

        private void ShouldPopViewController(object sender, CancelEventArgs args)
        {
            object parameter = null;
            var controllers = NavigationController.ViewControllers;
            if (controllers.Length > 1)
                parameter = controllers[controllers.Length - 2].GetNavigationParameter();
            args.Cancel = !RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Back, parameter));
        }

        private void DidPopViewController(object sender, EventArgs eventArgs)
        {
            var controller = NavigationController.TopViewController;
            RaiseNavigated(controller, NavigationMode.Back, controller.GetNavigationParameter());
        }

        private void RaiseNavigated(object content, NavigationMode mode, object parameter)
        {
            if (Navigated != null)
                RaiseNavigated(new NavigationEventArgs(content, parameter, mode));
        }

        private void ClearNavigationStackIfNeed(UIViewController newItem, IDataContext context)
        {
            if (context == null)
                context = DataContext.Empty;
            if (context.GetData(NavigationConstants.ClearBackStack) && NavigationController != null)
            {
                NavigationController.ViewControllers = new[] { newItem };
                context.AddOrUpdate(NavigationProvider.ClearNavigationCache, true);
            }
        }

        private void OnViewDidAppearHandler(UIViewController sender, ValueEventArgs<bool> args)
        {
            ((IViewControllerView)sender).Mediator.ViewDidAppearHandler -= OnViewDidAppearHandler;
            RaiseNavigated(sender, NavigationMode.New, sender.GetNavigationParameter());
        }

        #endregion
    }
}