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
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = System.Windows.Navigation.NavigationMode;
#if SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Interfaces.Navigation;
using MugenMvvmToolkit.Silverlight.Models.EventArg;

namespace MugenMvvmToolkit.Silverlight.Infrastructure.Navigation
#elif WINDOWS_PHONE
using System.ComponentModel;
using System.Linq;
using System.Windows;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.WinPhone.Interfaces.Navigation;
using MugenMvvmToolkit.WinPhone.Models.EventArg;

namespace MugenMvvmToolkit.WinPhone.Infrastructure.Navigation
#endif
{
    public class FrameNavigationService : INavigationService
    {
        #region Fields

        private readonly Frame _frame;
        private const string UriParameterString = "viewmodelparameter";

        #endregion

        #region Constructors

#if WINDOWS_PHONE
        public FrameNavigationService([NotNull] Frame frame, bool isRootFrame)
        {
            Should.NotBeNull(frame, nameof(frame));
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
            if (isRootFrame)
                PlatformExtensions.MainPageOnBackKeyPressed +=
                    ReflectionExtensions.CreateWeakEventHandler<FrameNavigationService, CancelEventArgs>(this,
                        (service, o, arg3) => OnBackButtonPressed(arg3), (o, handler) => PlatformExtensions.MainPageOnBackKeyPressed -= handler.Handle).Handle;
        }
#else
        public FrameNavigationService([NotNull] Frame frame)
        {
            Should.NotBeNull(frame, nameof(frame));
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }
#endif
        #endregion

        #region Methods

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            EventHandler<INavigationService, NavigationEventArgsBase> handler = Navigated;
            if (handler == null)
                return;
#if WINDOWS_PHONE
            args.SetHandled(true);
            args.InvokeAfterRestoreState(eventArgs => handler(this, new NavigationEventArgsWrapper(eventArgs)));
#else
            handler(this, new NavigationEventArgsWrapper(args));
#endif
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler != null)
                handler(this, new NavigatingCancelEventArgsWrapper(args));
        }

        private static string GetParameter(Uri uri)
        {
            IDictionary<string, string> parameters = uri.GetUriParameters();
            string value;
            parameters.TryGetValue(UriParameterString, out value);
            return value;
        }

        #endregion

        #region Implementation of INavigationService

        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        public object CurrentContent
        {
            get { return _frame.Content; }
        }

        public void GoBack()
        {
            _frame.GoBack();
        }

        public void GoForward()
        {
            _frame.GoForward();
        }

#if WINDOWS_PHONE
        public JournalEntry RemoveBackEntry()
        {
            var page = _frame.Content as Page;
            if (page != null && page.NavigationService != null)
                return page.NavigationService.RemoveBackEntry();
            return null;
        }
#endif

        public string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            var cancelArgs = args as NavigatingCancelEventArgsWrapper;
            if (cancelArgs != null)
                return GetParameter(cancelArgs.Args.Uri);
            var eventArgs = args as NavigationEventArgsWrapper;
            if (eventArgs == null)
                return null;
            return GetParameter(eventArgs.Args.Uri);
        }

        public bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, nameof(args));
#if WINDOWS_PHONE
            if (args is BackButtonNavigatingEventArgs)
            {
                var application = Application.Current;
                if (application == null)
                    return false;
                RaiseNavigated(BackButtonNavigationEventArgs.Instance);
                application.Terminate();
                return true;
            }
#endif
            bool result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            bool result = NavigateInternal(source, parameter);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var content = CurrentContent;
            return content != null && ViewManager.GetDataContext(content) == viewModel && CanGoBack;
        }

        public bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            if (CanClose(viewModel, dataContext))
            {
                GoBack();
                return true;
            }
            return false;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        private bool NavigateInternal(NavigatingCancelEventArgsBase args)
        {
            if (!args.IsCancelable)
                return false;
            NavigatingCancelEventArgs originalArgs = ((NavigatingCancelEventArgsWrapper)args).Args;
            if (originalArgs.NavigationMode == NavigationMode.Back)
            {
                _frame.GoBack();
                return true;
            }
            return _frame.Navigate(originalArgs.Uri);
        }

        private bool NavigateInternal(IViewMappingItem source, string parameter)
        {
            Uri uri = source.Uri;
            if (!string.IsNullOrEmpty(parameter))
            {
                uri =
                    uri.MergeUri(new[]
                    {
                        new KeyValuePair<string, string>(UriParameterString, parameter),
                        new KeyValuePair<string, string>("_timestamp",
                            DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture))
                    });
            }
            return _frame.Navigate(uri);
        }

#if WINDOWS_PHONE
        private void RaiseNavigated(NavigationEventArgsBase args)
        {
            var navigated = Navigated;
            if (navigated != null)
                navigated(this, args);
        }

        private void OnBackButtonPressed(CancelEventArgs args)
        {
            if (CanGoBack)
                return;
            var navigating = Navigating;
            if (navigating == null)
            {
                RaiseNavigated(BackButtonNavigationEventArgs.Instance);
                return;
            }
            var navArgs = new BackButtonNavigatingEventArgs();
            navigating(this, navArgs);
            args.Cancel = navArgs.Cancel;
        }
#endif

        private void ClearNavigationStackIfNeed(IDataContext context)
        {
#if WINDOWS_PHONE
            var page = _frame.Content as Page;
            if (page == null || page.NavigationService == null)
                return;
            var navigationService = page.NavigationService;
            if (context == null)
                context = DataContext.Empty;
            if (!context.GetData(NavigationConstants.ClearBackStack))
                return;
            while (navigationService.BackStack.OfType<object>().Any())
                navigationService.RemoveBackEntry();
            context.AddOrUpdate(NavigationProviderConstants.InvalidateAllCache, true);
#endif
        }

        #endregion
    }
}
