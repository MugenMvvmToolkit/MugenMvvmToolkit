#region Copyright

// ****************************************************************************
// <copyright file="NavigationService.cs">
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

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Models.EventArg;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using NavigationEventArgs = Xamarin.Forms.NavigationEventArgs;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private NavigationPage _rootPage;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationService()
        {
            XamarinFormsToolkitExtensions.BackButtonPressed += ReflectionExtensions
                .CreateWeakDelegate<NavigationService, CancelEventArgs, EventHandler<Page, CancelEventArgs>>(this,
                    (service, o, arg3) => service.OnBackButtonPressed((Page)o, arg3), (o, handler) => XamarinFormsToolkitExtensions.BackButtonPressed -= handler,
                    handler => handler.Handle);
            UseAnimations = true;
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        private Page CurrentContent => _rootPage?.CurrentPage;

        #endregion

        #region Implementation of INavigationService

        object INavigationService.CurrentContent => CurrentContent;

        public void UpdateRootPage(NavigationPage page, IViewModel rootPageViewModel)
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
            RaiseRootPageChanged(rootPageViewModel);
        }

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (!args.IsCancelable)
                return false;
            if (args.NavigationMode == NavigationMode.Remove)
                return TryClose(args.Context);

            var eventArgs = (NavigatingCancelEventArgs)args;
            //Back button pressed.
            if (eventArgs.IsBackButtonNavigation)
            {
                var sendBackButton = XamarinFormsToolkitExtensions.SendBackButtonPressed?.Invoke(CurrentContent);
                if (sendBackButton != null)
                {
                    sendBackButton();
                    RaiseNavigated(null, null, NavigationMode.Back, args.Context);
                    return true;
                }
            }

            if (eventArgs.NavigationMode == NavigationMode.Back)
                return GoBack(args.Context);
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, args.Context);
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            Should.NotBeNull(dataContext, nameof(dataContext));
            if (_rootPage == null)
                return false;
            bool bringToFront;
            dataContext.TryGetData(NavigationProvider.BringToFront, out bringToFront);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter, true, false, dataContext)))
                return false;

            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            bool animated;
            if (dataContext.TryGetData(NavigationConstants.UseAnimations, out animated))
                viewModel?.Settings.State.AddOrUpdate(NavigationConstants.UseAnimations, animated);
            else
                animated = UseAnimations;
            Page page = null;
            if (bringToFront && viewModel != null)
            {
                var navigation = _rootPage.Navigation;
                if (navigation != null)
                {
                    for (int i = 0; i < navigation.NavigationStack.Count; i++)
                    {
                        var p = navigation.NavigationStack[i];
                        if (p.BindingContext == viewModel)
                        {
                            page = p;
                            navigation.RemovePage(p);
                            break;
                        }
                    }
                }
            }
            if (page == null)
            {
                if (viewModel == null)
                    page = (Page)ServiceProvider.ViewManager.GetViewAsync(source, dataContext).Result;
                else
                    page = (Page)ServiceProvider.ViewManager.GetOrCreateView(viewModel, null, dataContext);
            }
            page.SetNavigationParameter(parameter);
            page.SetNavigationContext(dataContext, false);
            page.SetBringToFront(bringToFront);
            ClearNavigationStackIfNeed(dataContext, page, _rootPage.PushAsync(page, animated));
            return true;
        }

        public bool CanClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;

            var navigationStack = _rootPage.Navigation?.NavigationStack;
            if (navigationStack == null || navigationStack.Count <= 1)
                return false;

            if (CurrentContent?.BindingContext == viewModel)
                return true;

            for (var index = 0; index < navigationStack.Count; index++)
            {
                if (navigationStack[index].BindingContext == viewModel)
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

            if (CurrentContent != null && CurrentContent.BindingContext == viewModel)
                return GoBack(dataContext);

            var navigation = _rootPage.Navigation;
            if (navigation == null || !CanClose(dataContext))
                return false;

            if (RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Remove, null, true, false, dataContext)))
            {
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
                if (result)
                    RaiseNavigated(null, null, NavigationMode.Remove, dataContext);
                return result;
            }
            return true;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        public event EventHandler<INavigationService, ValueEventArgs<IViewModel>> RootPageChanged;

        #endregion

        #region Methods

        protected virtual bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler == null)
                return true;
            handler(this, args);
            return !args.Cancel;
        }

        protected virtual void RaiseNavigated(object page, string parameter, NavigationMode mode, IDataContext context)
        {
            Navigated?.Invoke(this, new Models.EventArg.NavigationEventArgs(page, parameter, mode, context));
        }

        protected virtual void RaiseRootPageChanged(IViewModel viewModel)
        {
            RootPageChanged?.Invoke(this, new ValueEventArgs<IViewModel>(viewModel));
        }

        private void OnPopped(object sender, NavigationEventArgs args)
        {
            RaiseNavigated(CurrentContent, CurrentContent.GetNavigationParameter(), NavigationMode.Back, CurrentContent.GetNavigationContext(true, true));
        }

        private void OnPushed(object sender, NavigationEventArgs args)
        {
            RaiseNavigated(args.Page, args.Page.GetNavigationParameter(), args.Page.GetBringToFront() ? NavigationMode.Refresh : NavigationMode.New, args.Page.GetNavigationContext(false, true));
        }

        private bool GoBack(IDataContext context)
        {
            var navigationStack = _rootPage?.Navigation?.NavigationStack;
            if (navigationStack == null || navigationStack.Count <= 1)
                return false;
            if (RaiseNavigating(new NavigatingCancelEventArgs(null, NavigationMode.Back, null, true, false, context)))
            {
                SetBackNavigationContext(context);
                _rootPage.PopAsync(IsAnimated(context, CurrentContent?.BindingContext as IViewModel));
            }
            return true;
        }

        private void OnBackButtonPressed(Page page, CancelEventArgs args)
        {
            if (CurrentContent != page)
                return;

            var navigationStack = _rootPage.Navigation?.NavigationStack;
            if (navigationStack == null || navigationStack.Count == 0)
                return;
            bool isBack = false;
            if (navigationStack.Count == 1)
            {
                if (XamarinFormsToolkitExtensions.SendBackButtonPressed == null)
                    return;
                isBack = true;
            }
            var eventArgs = new NavigatingCancelEventArgs(null, NavigationMode.Back, null, true, isBack, null);
            RaiseNavigating(eventArgs);
            args.Cancel = eventArgs.Cancel;
            if (!args.Cancel && isBack)
                RaiseNavigated(null, null, NavigationMode.Back, DataContext.Empty);
        }

        private void ClearNavigationStackIfNeed(IDataContext context, Page page, Task task)
        {
            var navigation = _rootPage.Navigation;
            if (navigation == null || context == null || !context.GetData(NavigationConstants.ClearBackStack))
                return;
            task.TryExecuteSynchronously(t =>
            {
                var pages = navigation.NavigationStack.ToList();
                pages.Reverse();
                for (int i = 0; i < pages.Count; i++)
                {
                    var toRemove = pages[i];
                    if (toRemove == page)
                        continue;
                    navigation.RemovePage(toRemove);
                    var viewModel = toRemove.BindingContext as IViewModel;
                    if (viewModel != null)
                    {
                        var ctx = new DataContext(context);
                        ctx.AddOrUpdate(NavigationConstants.ViewModel, viewModel);
                        RaiseNavigated(toRemove, null, NavigationMode.Remove, ctx);
                    }
                }
            });
        }

        private void SetBackNavigationContext(IDataContext context)
        {
            if (context == null)
                return;
            var navigationStack = _rootPage?.Navigation?.NavigationStack;
            if (navigationStack != null && navigationStack.Count > 1)
            {
                var page = navigationStack[navigationStack.Count - 2];
                page.SetNavigationContext(context, true);
            }
        }

        private bool IsAnimated(IDataContext context, IViewModel viewModel)
        {
            bool result;
            if (context != null && context.TryGetData(NavigationConstants.UseAnimations, out result))
                return result;
            if (viewModel != null && viewModel.Settings.State.TryGetData(NavigationConstants.UseAnimations, out result))
                return result;
            return UseAnimations;
        }

        #endregion
    }
}
