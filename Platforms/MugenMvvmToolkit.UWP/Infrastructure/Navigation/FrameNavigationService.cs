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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
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

        private readonly Frame _frame;
        private readonly IViewModelProvider _viewModelProvider;
        private IDataContext _lastContext;
        private bool _bringToFront;

        #endregion

        #region Constructors

        public FrameNavigationService(Frame frame, IViewModelProvider viewModelProvider)
        {
            Should.NotBeNull(frame, nameof(frame));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            _frame = frame;
            _viewModelProvider = viewModelProvider;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }

        #endregion

        #region Implementation of INavigationService

        public object CurrentContent => _frame.Content;

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (!args.IsCancelable)
                return false;

            if (args.NavigationMode == MugenMvvmToolkit.Models.NavigationMode.Remove)
                return TryClose(args.Context);

            if (args is RemoveNavigatingCancelEventArgs)
                return TryClose(args.Context);

            var wrapper = (NavigatingCancelEventArgsWrapper)args;
            if (wrapper.Args.NavigationMode == NavigationMode.Back)
            {
                _lastContext = args.Context;
                _frame.GoBack();
                return true;
            }
            return Navigate(wrapper.Args.SourcePageType, wrapper.Parameter, args.Context);
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            Should.NotBeNull(dataContext, nameof(dataContext));
            return Navigate(source.ViewType, parameter, dataContext);
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

            if (RaiseNavigatingRemove(dataContext))
            {
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
                if (closed)
                    RaiseNavigated(new RemoveNavigationEventArgs(dataContext));
                return closed;
            }
            return true;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        private void ClearNavigationStack(IDataContext context)
        {
            var backStack = _frame.BackStack;
            if (backStack.IsReadOnly)
                return;
            for (int index = 0; index < backStack.Count; index++)
            {
                var vmId = GetViewModelIdFromParameter(backStack[index].Parameter);
                var viewModel = _viewModelProvider.TryGetViewModelById(vmId);
                if (viewModel != null)
                {
                    var ctx = new DataContext(context);
                    ctx.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
                    RaiseNavigated(new RemoveNavigationEventArgs(ctx));
                }
                backStack.RemoveAt(index);
                --index;
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            var bringToFront = _bringToFront;
            var lastContext = _lastContext;
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
            if (bringToFront)
            {
                var id = GetViewModelIdFromParameter(args.Parameter);
                var backStack = _frame.BackStack;
                for (int index = 0; index < backStack.Count; index++)
                {
                    if (GetViewModelIdFromParameter(backStack[index].Parameter) == id)
                    {
                        backStack.RemoveAt(index);
                        break;
                    }
                }
            }
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            Navigating?.Invoke(this, new NavigatingCancelEventArgsWrapper(args, args.Parameter as string, _bringToFront, _lastContext));
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

        private bool Navigate(Type type, string parameter, IDataContext context)
        {
            context.TryGetData(NavigationProvider.BringToFront, out _bringToFront);
            var clearBackStack = context.GetData(NavigationConstants.ClearBackStack);
            _lastContext = context;
            var result = parameter == null ? _frame.Navigate(type) : _frame.Navigate(type, parameter);
            if (result && clearBackStack)
                ClearNavigationStack(context);
            return result;
        }

        private static Guid GetViewModelIdFromParameter(object parameter)
        {
            Guid id;
            NavigationProvider.GetViewModelTypeFromParameter(parameter as string, out id);
            return id;
        }

        #endregion
    }
}