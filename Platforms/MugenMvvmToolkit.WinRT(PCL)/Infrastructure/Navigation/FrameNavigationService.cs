#region Copyright

// ****************************************************************************
// <copyright file="FrameNavigationService.cs">
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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = Windows.UI.Xaml.Navigation.NavigationMode;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    /// <summary>
    ///     A basic implementation of <see cref="INavigationService" /> to adapt the <see cref="Frame" />.
    /// </summary>
    public class FrameNavigationService : INavigationService
    {
        #region Fields

        private readonly Frame _frame;
        private object _lastParameter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FrameNavigationService" /> class.
        /// </summary>
        public FrameNavigationService(Frame frame)
        {
            Should.NotBeNull(frame, "frame");
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }

        #endregion

        #region Methods

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            _lastParameter = null;
            var handler = Navigated;
            if (handler == null)
                return;

            var dp = args.Content as DependencyObject;
            if (dp == null)
                handler(this, new NavigationEventArgsWrapper(args));
            else
            {
                //to indicate that args is handled.
                args.SetHandled(true);
                //to restore state before navigate.
                dp.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => handler(this, new NavigationEventArgsWrapper(args)));
            }
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            var handler = Navigating;
            if (handler != null)
                handler(this, new NavigatingCancelEventArgsWrapper(args, _lastParameter));
        }

        private bool Navigate(Type type, object parameter)
        {
            if (parameter == null)
                return _frame.Navigate(type);
            return _frame.Navigate(type, parameter);
        }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        /// <summary>
        ///     The current content.
        /// </summary>
        public object CurrentContent
        {
            get { return _frame.Content; }
        }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public void GoBack()
        {
            _frame.GoBack();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _frame.GoForward();
        }

        /// <summary>
        ///     Gets a navigation parameter from event args.
        /// </summary>
        public object GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            var cancelEventArgs = args as NavigatingCancelEventArgsWrapper;
            if (cancelEventArgs == null)
            {
                var eventArgs = args as NavigationEventArgsWrapper;
                if (eventArgs == null)
                    return null;
                return eventArgs.Args.Parameter;
            }
            return cancelEventArgs.Parameter;
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
        public bool Navigate(IViewMappingItem source, object parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            _lastParameter = parameter;
            var result = Navigate(source.ViewType, parameter);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
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

        private bool NavigateInternal(NavigatingCancelEventArgsBase args)
        {
            var wrapper = (NavigatingCancelEventArgsWrapper)args;
            if (wrapper.Args.NavigationMode == NavigationMode.Back)
            {
                _frame.GoBack();
                return true;
            }
            return Navigate(wrapper.Args.SourcePageType, wrapper.Parameter);
        }

        private void ClearNavigationStackIfNeed(IDataContext context)
        {
#if WINDOWSCOMMON
            if (context == null)
                context = DataContext.Empty;
            if (!context.GetData(NavigationConstants.ClearBackStack) || _frame.BackStack.IsReadOnly)
                return;
            _frame.BackStack.Clear();
            context.AddOrUpdate(NavigationProvider.ClearNavigationCache, true);
#endif
        }

        #endregion
    }
}