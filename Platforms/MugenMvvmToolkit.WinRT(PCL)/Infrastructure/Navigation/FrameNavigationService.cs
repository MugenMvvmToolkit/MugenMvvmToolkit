#region Copyright

// ****************************************************************************
// <copyright file="FrameNavigationService.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
        private bool _bringToFront;

        #endregion

        #region Constructors

        public FrameNavigationService(Frame frame, bool isRootFrame = false)
        {
            Should.NotBeNull(frame, nameof(frame));
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
            if (isRootFrame)
            {
                var backPressedEventDelegate = PlatformExtensions.SubscribeBackPressedEventDelegate;
                if (backPressedEventDelegate != null)
                    backPressedEventDelegate(this, (o, sender, args) => ((FrameNavigationService)o).OnBackButtonPressed(args));
            }
        }

        #endregion

        #region Implementation of INavigationService

        public bool CanGoBack => _frame.CanGoBack;

        public bool CanGoForward => _frame.CanGoForward;

        public object CurrentContent => _frame.Content;

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
            Should.NotBeNull(args, nameof(args));
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
            Should.NotBeNull(args, nameof(args));
            if (args is BackButtonNavigatingEventArgs)
            {
                var application = Application.Current;
                if (application == null)
                    return false;
                RaiseNavigated(BackButtonNavigationEventArgs.Instance);
                application.Exit();
                return true;
            }

            var result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            if (dataContext == null)
                dataContext = DataContext.Empty;
            dataContext.TryGetData(NavigationProviderConstants.BringToFront, out _bringToFront);
            var bringToFront = _bringToFront;
            var result = Navigate(source.ViewType, parameter, dataContext.GetData(NavigationConstants.ViewModel));
            if (result)
            {
                if (bringToFront)
                    dataContext.AddOrUpdate(NavigationProviderConstants.InvalidateCache, true);
                ClearNavigationStackIfNeed(dataContext);
            }
            return result;
        }

        public bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var content = CurrentContent;
            var canClose = content != null && ViewManager.GetDataContext(content) == viewModel && CanGoBack;
            if (canClose)
                return true;
            var viewModelId = viewModel.GetViewModelId();
            for (int index = 0; index < _frame.BackStack.Count; index++)
            {
                if (GetViewModelIdFromParameter(_frame.BackStack[index].Parameter) == viewModelId)
                    return true;
            }
            return false;
        }

        public bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var content = CurrentContent;
            if (content != null && ViewManager.GetDataContext(content) == viewModel && CanGoBack)
            {
                GoBack();
                return true;
            }
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
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        private void OnBackButtonPressed(object args)
        {
            if (CanGoBack)
                return;
            var backPressedHandledDelegate = PlatformExtensions.SetBackPressedHandledDelegate;
            if (backPressedHandledDelegate == null)
            {
                RaiseNavigated(BackButtonNavigationEventArgs.Instance);
                return;
            }

            var navigating = Navigating;
            if (navigating == null)
            {
                RaiseNavigated(BackButtonNavigationEventArgs.Instance);
                return;
            }

            var navArgs = new BackButtonNavigatingEventArgs();
            navigating(this, navArgs);
            backPressedHandledDelegate(args, navArgs.Cancel);
        }

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

        private void ClearNavigationStackIfNeed(IDataContext context)
        {
            if (context == null)
                context = DataContext.Empty;
            if (!context.GetData(NavigationConstants.ClearBackStack) || _frame.BackStack.IsReadOnly)
                return;
            _frame.BackStack.Clear();
            context.AddOrUpdate(NavigationProviderConstants.InvalidateAllCache, true);
        }

        private static string GetParameter(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                return parameter;
            var index = parameter.IndexOf(IdSeparator[0], StringComparison.Ordinal);
            if (index < 0)
                return parameter;
            return parameter.Substring(index + IdSeparator[0].Length);
        }

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            var bringToFront = _bringToFront;
            _lastParameter = null;
            _bringToFront = false;
            var handler = Navigated;
            if (handler == null)
                return;

            var dp = args.Content as DependencyObject;
            if (dp == null)
                handler(this, new NavigationEventArgsWrapper(args, bringToFront));
            else
            {
                //to indicate that args is handled.
                args.SetHandled(true);
                //to restore state before navigate.
                dp.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => handler(this, new NavigationEventArgsWrapper(args, bringToFront)));
            }
            if (!bringToFront)
                return;
            var id = GetViewModelIdFromParameter(args.Parameter);
            for (int index = 0; index < _frame.BackStack.Count; index++)
            {
                if (GetViewModelIdFromParameter(_frame.BackStack[index].Parameter) == id)
                {
                    _frame.BackStack.RemoveAt(index);
                    --index;
                    break;
                }
            }
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            Navigating?.Invoke(this, new NavigatingCancelEventArgsWrapper(args, _lastParameter, _bringToFront));
        }

        private void RaiseNavigated(NavigationEventArgsBase args)
        {
            Navigated?.Invoke(this, args);
        }

        private bool Navigate(Type type, string parameter, IViewModel viewModel)
        {
            if (viewModel != null)
            {
                if (parameter == null)
                    parameter = viewModel.GetViewModelId().ToString();
                else
                    parameter = viewModel.GetViewModelId().ToString() + IdSeparator[0] + parameter;
            }
            _lastParameter = parameter;
            if (parameter == null)
                return _frame.Navigate(type);
            return _frame.Navigate(type, parameter);
        }

        #endregion
    }
}
