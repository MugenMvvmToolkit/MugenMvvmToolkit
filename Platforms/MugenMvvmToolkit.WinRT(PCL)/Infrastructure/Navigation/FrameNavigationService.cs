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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.WinRT.Interfaces.Navigation;
using MugenMvvmToolkit.WinRT.Models.EventArg;
using NavigationMode = Windows.UI.Xaml.Navigation.NavigationMode;

namespace MugenMvvmToolkit.WinRT.Infrastructure.Navigation
{
    public class FrameNavigationService : INavigationService
    {

        #region Fields

        private static readonly string[] IdSeparator = { "~n|s~" };
        private readonly Frame _frame;
        private string _lastParameter;

        #endregion

        #region Constructors

        public FrameNavigationService(Frame frame)
        {
            Should.NotBeNull(frame, "frame");
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
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

        public string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            var cancelEventArgs = args as NavigatingCancelEventArgsWrapper;
            if (cancelEventArgs == null)
            {
                var eventArgs = args as NavigationEventArgsWrapper;
                if (eventArgs == null)
                    return null;
                return GetParameter(eventArgs.Args.Parameter as string);
            }
            return GetParameter(cancelEventArgs.Parameter);
        }

        public bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, "args");
            var result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            if (dataContext == null)
                dataContext = DataContext.Empty;
            var result = Navigate(source.ViewType, parameter, dataContext.GetData(NavigationConstants.ViewModel));
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var content = CurrentContent;
            var canClose = content != null && ViewManager.GetDataContext(content) == viewModel && CanGoBack;
            if (canClose)
                return true;
#if WINDOWSCOMMON
            var viewModelId = viewModel.GetViewModelId();
            for (int index = 0; index < _frame.BackStack.Count; index++)
            {
                if (GetViewModelIdFromParameter(_frame.BackStack[index].Parameter) == viewModelId)
                    return true;
            }
#endif
            return false;
        }

        public bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var content = CurrentContent;
            if (content != null && ViewManager.GetDataContext(content) == viewModel && CanGoBack)
            {
                GoBack();
                return true;
            }
#if WINDOWSCOMMON
            bool closed = false;
            var viewModelId = viewModel.GetViewModelId();
            for (int index = 0; index < _frame.BackStack.Count; index++)
            {
                if (GetViewModelIdFromParameter(_frame.BackStack[index].Parameter) == viewModelId)
                {
                    _frame.BackStack.RemoveAt(index);
                    --index;
                    closed = true;
                }
            }
            return closed;
#else
            return false;
#endif
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        private bool NavigateInternal(NavigatingCancelEventArgsBase args)
        {
            if (!args.IsCancelable)
                return false;
            var wrapper = (NavigatingCancelEventArgsWrapper)args;
            if (wrapper.Args.NavigationMode == NavigationMode.Back)
            {
                _frame.GoBack();
                return true;
            }
            return Navigate(wrapper.Args.SourcePageType, wrapper.Parameter, null);
        }

#if WINDOWSCOMMON
        private static Guid GetViewModelIdFromParameter(object parameter)
        {
            var s = parameter as string;
            if (string.IsNullOrEmpty(s))
                return Guid.Empty;
            if (s.Contains(IdSeparator[0]))
                s = s.Split(IdSeparator, StringSplitOptions.RemoveEmptyEntries)[0];
            Guid id;
            Guid.TryParse(s, out id);
            return id;
        }
#endif

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

        private static string GetParameter(string parameter)
        {
#if WINDOWSCOMMON
            if (string.IsNullOrEmpty(parameter))
                return parameter;
            var index = parameter.IndexOf(IdSeparator[0], StringComparison.Ordinal);
            if (index < 0)
                return parameter;
            return parameter.Substring(index + IdSeparator[0].Length);
#else
            return parameter;
#endif
        }

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

        private bool Navigate(Type type, string parameter, IViewModel viewModel)
        {
#if WINDOWSCOMMON
            if (viewModel != null)
            {
                if (parameter == null)
                    parameter = viewModel.GetViewModelId().ToString();
                else
                    parameter = viewModel.GetViewModelId().ToString() + IdSeparator[0] + parameter;
            }
#endif
            _lastParameter = parameter;
            if (parameter == null)
                return _frame.Navigate(type);
            return _frame.Navigate(type, parameter);
        }

        #endregion
    }
}
