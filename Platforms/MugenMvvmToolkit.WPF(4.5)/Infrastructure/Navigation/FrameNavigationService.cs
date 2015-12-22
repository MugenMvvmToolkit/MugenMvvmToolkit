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
using System.Linq;
using System.Windows.Controls;
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
    public class FrameNavigationService : INavigationService
    {
        #region Fields

        private readonly Frame _frame;
        private readonly bool _useUrlNavigation;
        private NavigationMode _lastMode;

        #endregion

        #region Constructors

        public FrameNavigationService([NotNull] Frame frame, bool useUrlNavigation)
        {
            Should.NotBeNull(frame, nameof(frame));
            _frame = frame;
            _useUrlNavigation = useUrlNavigation;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
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

        public JournalEntry RemoveBackEntry()
        {
            return _frame.RemoveBackEntry();
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
                return eventArgs.Args.ExtraData as string;
            }
            return cancelEventArgs.Args.ExtraData as string;
        }

        public bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, nameof(args));
            var result = NavigateInternal(args);
            if (result)
                ClearNavigationStackIfNeed(dataContext);
            return result;
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            var result = NavigateInternal(source, parameter);
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
            while (_frame.BackStack.OfType<object>().Any())
                _frame.RemoveBackEntry();
            context.AddOrUpdate(NavigationProviderConstants.InvalidateAllCache, true);
        }

        private bool NavigateInternal(IViewMappingItem source, object parameter)
        {
            if (_useUrlNavigation)
            {
                if (parameter == null)
                    return _frame.Navigate(source.Uri);
                return _frame.Navigate(source.Uri, parameter);
            }
            if (parameter == null)
                return _frame.Navigate(CreateView(source, null));
            return _frame.Navigate(CreateView(source, parameter), parameter);
        }

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
            if (_useUrlNavigation)
            {
                if (originalArgs.ExtraData == null)
                    return _frame.Navigate(originalArgs.Uri);
                return _frame.Navigate(originalArgs.Uri, originalArgs.ExtraData);
            }
            if (originalArgs.ExtraData == null)
                return _frame.Navigate(originalArgs.Content);
            return _frame.Navigate(originalArgs.Content, originalArgs.ExtraData);
        }

        #endregion
    }
}
