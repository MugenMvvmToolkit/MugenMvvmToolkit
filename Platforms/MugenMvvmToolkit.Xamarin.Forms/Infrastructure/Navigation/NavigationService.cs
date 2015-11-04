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
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Models.EventArg;
using Xamarin.Forms;
using NavigationEventArgs = Xamarin.Forms.NavigationEventArgs;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IThreadManager _threadManager;
        private readonly bool _isRootFrame;
        private NavigationPage _rootPage;

        #endregion

        #region Constructors

        public NavigationService([NotNull] IThreadManager threadManager, bool isRootFrame = false)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
            _isRootFrame = isRootFrame;
            XamarinFormsExtensions.BackButtonPressed += ReflectionExtensions
                .CreateWeakDelegate<NavigationService, CancelEventArgs, EventHandler<Page, CancelEventArgs>>(this,
                    (service, o, arg3) => service.OnBackButtonPressed((Page)o, arg3), (o, handler) => XamarinFormsExtensions.BackButtonPressed -= handler,
                    handler => handler.Handle);
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        #endregion

        #region Implementation of INavigationService

        public virtual bool CanGoBack
        {
            get { return _rootPage.Navigation != null && _rootPage.Navigation.NavigationStack.Count > 1; }
        }

        public virtual bool CanGoForward
        {
            get { return false; }
        }

        public virtual object CurrentContent
        {
            get
            {
                if (_rootPage == null)
                    return null;
                return _rootPage.CurrentPage;
            }
        }

        public virtual void GoBack()
        {
            if (_rootPage != null)
            {
                bool animated;
                var viewModel = CurrentContent == null ? null : CurrentContent.DataContext() as IViewModel;
                if (viewModel == null || !viewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out animated))
                    animated = UseAnimations;
                _rootPage.PopAsync(animated);
            }
        }

        public virtual void GoForward()
        {
            throw new NotSupportedException();
        }

        public virtual void UpdateRootPage(NavigationPage page)
        {
            if (_rootPage != null)
            {
                _rootPage.Pushed -= OnPushed;
                _rootPage.Popped -= OnPopped;
                _rootPage.PoppedToRoot -= OnPopped;
            }
            if (page != null)
            {
                page.Pushed += OnPushed;
                page.Popped += OnPopped;
                page.PoppedToRoot += OnPopped;
            }
            _rootPage = page;

            var currentPage = CurrentContent as Page ?? page;
            if (currentPage != null)
            {
                var context = currentPage.DataContext();
                if (context != null)
                    currentPage.SetNavigationParameter(NavigationProvider.GenerateNavigationParameter(context.GetType(), string.Empty));
                _threadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, currentPage,
                    (service, p) => service.RaiseNavigated(p, p.GetNavigationParameter() as string, NavigationMode.New),
                    OperationPriority.Low);
            }
        }

        public virtual string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            var cancelArgs = args as NavigatingCancelEventArgs;
            if (cancelArgs == null)
            {
                var eventArgs = args as Models.EventArg.NavigationEventArgs;
                if (eventArgs == null)
                    return null;
                return eventArgs.Parameter;
            }
            return cancelArgs.Parameter;
        }

        public virtual bool Navigate(NavigatingCancelEventArgsBase args, IDataContext context)
        {
            Should.NotBeNull(args, "args");
            if (!args.IsCancelable)
                return false;
            var eventArgs = (NavigatingCancelEventArgs)args;
            //Close button pressed.
            if (eventArgs.IsBackButtonNavigation && XamarinFormsExtensions.SendBackButtonPressed != null)
            {
                var sendBackButton = XamarinFormsExtensions.SendBackButtonPressed(CurrentContent as Page);
                if (sendBackButton != null)
                {
                    RaiseNavigated(null, null, NavigationMode.Back);
                    sendBackButton();
                    return true;
                }
            }
            if (eventArgs.NavigationMode == NavigationMode.Back)
            {
                GoBack();
                return true;
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, context);
        }

        public virtual bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            if (_rootPage == null)
                return false;
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, NavigationMode.New, parameter, true, false)))
                return false;
            if (dataContext == null)
                dataContext = DataContext.Empty;

            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            bool animated;
            if (dataContext.TryGetData(NavigationConstants.UseAnimations, out animated))
            {
                if (viewModel != null)
                    viewModel.Settings.State.AddOrUpdate(NavigationConstants.UseAnimations, animated);
            }
            else
                animated = UseAnimations;
            Page page;
            if (viewModel == null)
                page = (Page)ServiceProvider.Get<IViewManager>().GetViewAsync(source, dataContext).Result;
            else
                page = (Page)ViewManager.GetOrCreateView(viewModel, null, dataContext);
            page.SetNavigationParameter(parameter);
            ClearNavigationStackIfNeed(dataContext, page, _rootPage.PushAsync(page, animated));
            return true;
        }

        public virtual bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var navigation = _rootPage.Navigation;
            if (navigation == null)
                return false;
            foreach (var page in navigation.NavigationStack)
            {
                if (page.DataContext() == viewModel)
                    return true;
            }
            return false;
        }

        public virtual bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (CurrentContent != null && CurrentContent.DataContext() == viewModel)
            {
                GoBack();
                return true;
            }
            var navigation = _rootPage.Navigation;
            if (navigation == null)
                return false;
            bool result = false;
            var pages = navigation.NavigationStack.ToList();
            for (int i = 0; i < pages.Count; i++)
            {
                var toRemove = pages[i];
                if (toRemove.BindingContext == viewModel)
                {
                    navigation.RemovePage(toRemove);
                    pages.RemoveAt(i);
                    result = true;
                    --i;
                }
            }
            return result;
        }

        public virtual event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public virtual event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion

        #region Methods

        protected bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler == null)
                return true;
            handler(this, args);
            return !args.Cancel;
        }

        protected void RaiseNavigated(object page, string parameter, NavigationMode mode)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new Models.EventArg.NavigationEventArgs(page, parameter, mode));
        }

        private void OnPopped(object sender, NavigationEventArgs args)
        {
            var page = CurrentContent as Page;
            RaiseNavigated(CurrentContent, page.GetNavigationParameter() as string, NavigationMode.Back);
        }

        private void OnPushed(object sender, NavigationEventArgs args)
        {
            RaiseNavigated(args.Page, args.Page.GetNavigationParameter() as string, NavigationMode.New);
        }

        private void OnBackButtonPressed(Page page, CancelEventArgs args)
        {
            if (CurrentContent != page)
                return;

            var isCancelable = CanGoBack || (_isRootFrame && XamarinFormsExtensions.SendBackButtonPressed != null);
            if (isCancelable)
            {
                var eventArgs = new NavigatingCancelEventArgs(null, NavigationMode.Back, null, true, true);
                RaiseNavigating(eventArgs);
                args.Cancel = eventArgs.Cancel;
            }
            if (isCancelable && args.Cancel)
                return;

            if (!CanGoBack)
                RaiseNavigated(null, null, NavigationMode.Back);
        }

        private void ClearNavigationStackIfNeed(IDataContext context, Page page, Task task)
        {
            var navigation = _rootPage.Navigation;
            if (navigation == null || context == null || !context.GetData(NavigationConstants.ClearBackStack))
                return;
            task.TryExecuteSynchronously(t =>
            {
                var pages = navigation.NavigationStack.ToList();
                for (int i = 0; i < pages.Count; i++)
                {
                    var toRemove = pages[i];
                    if (toRemove != page)
                        navigation.RemovePage(toRemove);
                }
            });
            context.AddOrUpdate(NavigationProvider.ClearNavigationCache, true);
        }

        #endregion
    }
}
