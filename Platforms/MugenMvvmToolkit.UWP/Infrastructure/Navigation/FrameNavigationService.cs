#region Copyright

// ****************************************************************************
// <copyright file="FrameNavigationService.cs">
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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;
using MugenMvvmToolkit.UWP.Models.EventArg;
using NavigationMode = Windows.UI.Xaml.Navigation.NavigationMode;

namespace MugenMvvmToolkit.UWP.Infrastructure.Navigation
{
    public class FrameNavigationService : INavigationService
    {
        #region Fields

        private static readonly string[] IdSeparator = { "~n|s~" };
        private readonly Frame _frame;
        private string _lastParameter;
        private IDataContext _lastContext;
        private bool _bringToFront;

        #endregion

        #region Constructors

        public FrameNavigationService(Frame frame)
        {
            Should.NotBeNull(frame, nameof(frame));
            _frame = frame;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }

        #endregion

        #region Implementation of INavigationService

        public object CurrentContent => _frame.Content;

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

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (args.NavigationMode == MugenMvvmToolkit.Models.NavigationMode.Remove && args.Context != null)
                return TryClose(args.Context);

            var result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(args.Context);
            return result;
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            dataContext = dataContext.ToNonReadOnly();
            dataContext.TryGetData(NavigationProviderConstants.BringToFront, out _bringToFront);
            var bringToFront = _bringToFront;
            var result = Navigate(source.ViewType, parameter, dataContext.GetData(NavigationConstants.ViewModel), dataContext);
            if (result)
            {
                if (bringToFront)
                    dataContext.AddOrUpdate(NavigationProviderConstants.InvalidateCache, true);
                ClearNavigationStackIfNeed(dataContext);
            }
            return result;
        }

        public bool CanClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;

            var content = CurrentContent;
            var canClose = content != null && ToolkitExtensions.GetDataContext(content) == viewModel && _frame.CanGoBack;
            if (canClose)
                return true;
            var viewModelId = viewModel.GetViewModelId();
            var backStack = _frame.BackStack;
            for (int index = 0; index < backStack.Count; index++)
            {
                if (GetViewModelIdFromParameter(backStack[index].Parameter) == viewModelId)
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

            var content = CurrentContent;
            if (content != null && ToolkitExtensions.GetDataContext(content) == viewModel && _frame.CanGoBack)
            {
                _lastContext = dataContext;
                _frame.GoBack();
                return true;
            }

            if (!CanClose(dataContext))
                return false;

            bool closed = false;
            if (RaiseNavigatingRemove(dataContext))
            {
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
                if (closed)
                    RaiseNavigated(new RemoveNavigationEventArgs(dataContext));
            }
            return closed;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        private bool NavigateInternal(NavigatingCancelEventArgsBase args)
        {
            if (!args.IsCancelable)
                return false;
            if (args is RemoveNavigatingCancelEventArgs && args.Context != null)
                return TryClose(args.Context);
            var wrapper = (NavigatingCancelEventArgsWrapper)args;
            if (wrapper.Args.NavigationMode == NavigationMode.Back)
            {
                _lastContext = args.Context;
                _frame.GoBack();
                return true;
            }
            return Navigate(wrapper.Args.SourcePageType, wrapper.Parameter, null, args.Context);
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

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            var bringToFront = _bringToFront;
            var lastContext = _lastContext;
            _lastParameter = null;
            _lastContext = null;
            _bringToFront = false;
            var handler = Navigated;
            if (handler == null)
                return;

            var dp = args.Content as DependencyObject;
            if (dp == null)
                handler(this, new NavigationEventArgsWrapper(args, bringToFront, lastContext));
            else
            {
                //to indicate that args is handled.
                args.SetHandled(true);
                //to restore state before navigate.
                dp.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => handler(this, new NavigationEventArgsWrapper(args, bringToFront, lastContext)));
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
            Navigating?.Invoke(this, new NavigatingCancelEventArgsWrapper(args, _lastParameter, _bringToFront, _lastContext));
        }

        private bool RaiseNavigatingRemove(IDataContext context)
        {
            var args = new RemoveNavigatingCancelEventArgs(context);
            Navigating?.Invoke(this, args);
            if (args.Cancel)
                return false;
            return true;
        }

        private void RaiseNavigated(NavigationEventArgsBase args)
        {
            Navigated?.Invoke(this, args);
        }

        private bool Navigate(Type type, string parameter, IViewModel viewModel, IDataContext context)
        {
            if (viewModel != null)
            {
                if (parameter == null)
                    parameter = viewModel.GetViewModelId().ToString();
                else
                    parameter = viewModel.GetViewModelId().ToString() + IdSeparator[0] + parameter;
            }
            _lastParameter = parameter;
            _lastContext = context;
            if (parameter == null)
                return _frame.Navigate(type);
            return _frame.Navigate(type, parameter);
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

        private static string GetParameter(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                return parameter;
            var index = parameter.IndexOf(IdSeparator[0], StringComparison.Ordinal);
            if (index < 0)
                return parameter;
            return parameter.Substring(index + IdSeparator[0].Length);
        }

        #endregion
    }
}