#region Copyright

// ****************************************************************************
// <copyright file="WindowNavigationService.cs">
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
using System.Linq;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;
using MugenMvvmToolkit.WPF.Models.EventArg;
using NavigationMode = System.Windows.Navigation.NavigationMode;

namespace MugenMvvmToolkit.WPF.Infrastructure.Navigation
{
    /// <summary>
    ///     A basic implementation of <see cref="INavigationService" /> to adapt the <see cref="NavigationWindow" />.
    /// </summary>
    public class WindowNavigationService : INavigationService
    {
        #region Fields

        private readonly bool _useUrlNavigation;
        private readonly NavigationWindow _window;
        private NavigationMode _lastMode;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FrameNavigationService" /> class.
        /// </summary>
        public WindowNavigationService([NotNull] NavigationWindow window, bool useUrlNavigation)
        {
            Should.NotBeNull(window, "window");
            _window = window;
            _useUrlNavigation = useUrlNavigation;
            if (useUrlNavigation)
            {
                _window.Navigating += OnNavigating;
                _window.Navigated += OnNavigated;
            }
        }

        #endregion

        #region Methods

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new NavigationEventArgsWrapper(args, _lastMode.ToNavigationMode()));
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            _lastMode = args.NavigationMode;
            var handler = Navigating;
            if (handler != null)
                handler(this, new NavigatingCancelEventArgsWrapper(args));
        }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _window.CanGoBack; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _window.CanGoForward; }
        }

        /// <summary>
        ///     The current content.
        /// </summary>
        public object CurrentContent
        {
            get { return _window.Content; }
        }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public void GoBack()
        {
            _window.GoBack();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _window.GoForward();
        }

        /// <summary>
        ///     Removes the most recent entry from the back stack.
        /// </summary>
        /// <returns> The entry that was removed. </returns>
        public JournalEntry RemoveBackEntry()
        {
            return _window.RemoveBackEntry();
        }

        /// <summary>
        ///     Gets a navigation parameter from event args.
        /// </summary>
        public string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            var cancelEventArgs = args as NavigatingCancelEventArgsWrapper;
            if (cancelEventArgs == null)
            {
                var eventArgs = args as NavigationEventArgsWrapper;
                if (eventArgs == null)
                    return null;
                return eventArgs.Args.ExtraData as string;
            }
            return cancelEventArgs.Args.ExtraData as string;
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
        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            var result = NavigateInternal(source, parameter);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        /// <summary>
        ///     Determines whether the specified command <c>CloseCommand</c> can be execute.
        /// </summary>
        public bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var content = CurrentContent;
            return content != null && ViewManager.GetDataContext(content) == viewModel && CanGoBack;
        }

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        public bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, "args");
            var result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        /// <summary>
        ///     Tries to close view-model page.
        /// </summary>
        public bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            if (CanClose(viewModel, dataContext))
            {
                GoBack();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Raised prior to navigation.
        /// </summary>
        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        /// <summary>
        ///     Raised after navigation.
        /// </summary>
        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of view object.
        /// </summary>
        protected virtual object CreateView(IViewMappingItem viewMapping, object parameter)
        {
            return ServiceProvider.Get<IViewManager>().GetViewAsync(viewMapping, parameter as IDataContext).Result;
        }

        private void ClearNavigationStackIfNeed(IDataContext context)
        {
            if (context == null)
                context = DataContext.Empty;
            if (!context.GetData(NavigationConstants.ClearBackStack))
                return;
            while (_window.BackStack.OfType<object>().Any())
                _window.RemoveBackEntry();
            context.AddOrUpdate(NavigationProvider.ClearNavigationCache, true);
        }

        private bool NavigateInternal(IViewMappingItem source, object parameter)
        {
            if (_useUrlNavigation)
            {
                if (parameter == null)
                    return _window.Navigate(source.Uri);
                return _window.Navigate(source.Uri, parameter);
            }
            if (parameter == null)
                return _window.Navigate(CreateView(source, null));
            return _window.Navigate(CreateView(source, parameter), parameter);
        }

        private bool NavigateInternal(NavigatingCancelEventArgsBase args)
        {
            if (!args.IsCancelable)
                return false;
            NavigatingCancelEventArgs originalArgs = ((NavigatingCancelEventArgsWrapper)args).Args;
            if (originalArgs.NavigationMode == NavigationMode.Back)
            {
                _window.GoBack();
                return true;
            }
            if (_useUrlNavigation)
            {
                if (originalArgs.ExtraData == null)
                    return _window.Navigate(originalArgs.Uri);
                return _window.Navigate(originalArgs.Uri, originalArgs.ExtraData);
            }
            if (originalArgs.ExtraData == null)
                return _window.Navigate(originalArgs.Content);
            return _window.Navigate(originalArgs.Content, originalArgs.ExtraData);
        }

        #endregion
    }
}