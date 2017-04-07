#region Copyright

// ****************************************************************************
// <copyright file="WindowNavigationService.cs">
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
using System.Linq;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;
using MugenMvvmToolkit.WPF.Models.EventArg;
using NavigationMode = System.Windows.Navigation.NavigationMode;

namespace MugenMvvmToolkit.WPF.Infrastructure.Navigation
{
    public class WindowNavigationService : INavigationService
    {
        #region Fields

        private readonly NavigationWindow _window;
        private readonly bool _useUrlNavigation;
        private NavigationMode _lastMode;
        private IDataContext _lastContext;

        #endregion

        #region Constructors

        public WindowNavigationService([NotNull] NavigationWindow window, bool useUrlNavigation)
        {
            Should.NotBeNull(window, nameof(window));
            _window = window;
            _useUrlNavigation = useUrlNavigation;
            _window.Navigating += OnNavigating;
            _window.Navigated += OnNavigated;
        }

        #endregion

        #region Methods

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            Navigated?.Invoke(this, new NavigationEventArgsWrapper(args, _lastMode.ToNavigationMode(), _lastContext));
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            _lastMode = args.NavigationMode;
            Navigating?.Invoke(this, new NavigatingCancelEventArgsWrapper(args, _lastContext));
        }

        #endregion

        #region Implementation of INavigationService

        public object CurrentContent => _window.Content;

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            var result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(args.Context);
            return result;
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            var result = NavigateInternal(source, parameter, dataContext);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool CanClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            var content = CurrentContent;
            return content != null && ToolkitExtensions.GetDataContext(content) == viewModel && _window.CanGoBack;
        }

        public bool TryClose(IDataContext dataContext)
        {
            if (CanClose(dataContext))
            {
                _lastContext = dataContext;
                _window.GoBack();
                return true;
            }
            return false;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        protected virtual object CreateView(IViewMappingItem viewMapping, object parameter)
        {
            return ServiceProvider.ViewManager.GetViewAsync(viewMapping, parameter as IDataContext).Result;
        }

        private void ClearNavigationStackIfNeed(IDataContext context)
        {
            if (context == null)
                context = DataContext.Empty;
            if (!context.GetData(NavigationConstants.ClearBackStack))
                return;
            while (_window.BackStack.OfType<object>().Any())
                _window.RemoveBackEntry();
            context.AddOrUpdate(NavigationProviderConstants.InvalidateAllCache, true);
        }

        private bool NavigateInternal(IViewMappingItem source, object parameter, IDataContext context)
        {
            _lastContext = context;
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
                _lastContext = args.Context;
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
