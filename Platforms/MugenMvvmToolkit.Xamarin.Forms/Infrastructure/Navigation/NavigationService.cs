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
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using Xamarin.Forms;
using NavigationEventArgs = Xamarin.Forms.NavigationEventArgs;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly NavigationPage _rootPage;

        #endregion

        #region Constructors

        public NavigationService([NotNull] NavigationPage rootPage)
        {
            Should.NotBeNull(rootPage, "rootPage");
            _rootPage = rootPage;
            _rootPage.Pushed += OnPushed;
            _rootPage.Popped += OnPopped;
            _rootPage.PoppedToRoot += OnPopped;
            XamarinFormsExtensions.BackButtonPressed += ReflectionExtensions
                .CreateWeakDelegate<NavigationService, CancelEventArgs, EventHandler<Page, CancelEventArgs>>(this,
                    (service, o, arg3) => service.OnBackButtonPressed((Page)o, arg3),
                    (o, handler) => XamarinFormsExtensions.BackButtonPressed -= handler, handler => handler.Handle);
            //BUG: Xamarin forms removes the page incorrectly using the RemovePage method, possible in future versions it will be fixed
            IgnoreClearBackStackHint = true;
        }

        #endregion

        #region Properties

        public bool IgnoreClearBackStackHint { get; set; }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return CurrentContent != null; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return false; }
        }

        /// <summary>
        ///     The current content.
        /// </summary>
        public object CurrentContent
        {
            get { return _rootPage.CurrentPage; }
        }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public void GoBack()
        {
            _rootPage.PopAsync();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public void GoForward()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Gets a navigation parameter from event args.
        /// </summary>
        public object GetParameterFromArgs(EventArgs args)
        {
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

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        public bool Navigate(NavigatingCancelEventArgsBase args, IDataContext context)
        {
            if (!args.IsCancelable)
                return false;
            var eventArgs = ((NavigatingCancelEventArgs)args);
            if (eventArgs.NavigationMode == NavigationMode.Back)
            {
                GoBack();
                return true;
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            return Navigate(eventArgs.Mapping, eventArgs.Parameter, context);
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
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, NavigationMode.New, parameter)))
                return false;
            if (dataContext == null)
                dataContext = DataContext.Empty;

            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            Page page;
            if (viewModel == null)
                page = (Page)ServiceProvider.IocContainer.Get(source.ViewType);
            else
                page = (Page)ViewManager.GetOrCreateView(viewModel, null, dataContext);
            page.SetNavigationParameter(parameter);
            ClearNavigationStackIfNeed(dataContext, page, _rootPage.PushAsync(page));
            return true;
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

        private void OnPopped(object sender, NavigationEventArgs args)
        {
            var handler = Navigated;
            if (handler != null)
            {
                var page = CurrentContent as Page;
                handler(this, new Models.EventArg.NavigationEventArgs(CurrentContent, page.GetNavigationParameter(), NavigationMode.Back));
            }
        }

        private void OnPushed(object sender, NavigationEventArgs args)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new Models.EventArg.NavigationEventArgs(args.Page, args.Page.GetNavigationParameter(), NavigationMode.New));
        }

        private bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler == null)
                return true;
            handler(this, args);
            return !args.Cancel;
        }

        private void OnBackButtonPressed(Page page, CancelEventArgs args)
        {
            if (CurrentContent != page)
                return;
            var eventArgs = new NavigatingCancelEventArgs(null, NavigationMode.Back, null);
            RaiseNavigating(eventArgs);
            args.Cancel = eventArgs.Cancel;
        }
        
        private void ClearNavigationStackIfNeed(IDataContext context, Page page, Task task)
        {
            var navigation = _rootPage.Navigation;
            if (IgnoreClearBackStackHint || navigation == null || context == null || !context.GetData(NavigationConstants.ClearBackStack))
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