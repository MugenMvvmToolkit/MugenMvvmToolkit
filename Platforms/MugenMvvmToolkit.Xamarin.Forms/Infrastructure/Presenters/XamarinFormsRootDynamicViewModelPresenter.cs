#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsRootDynamicViewModelPresenter.cs">
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
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Models.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Presenters
{
    public class XamarinFormsRootDynamicViewModelPresenter : IDynamicViewModelPresenter,
        IHandler<BackgroundNavigationMessage>, IHandler<ForegroundNavigationMessage>, IHandler<ApplicationStartingMessage>
    {
        #region Fields

        private bool _hasRootPage;
        protected static readonly DataConstant<object> IsClosed;
        protected static readonly DataConstant<object> IsRootConstant;

        #endregion

        #region Constructors

        static XamarinFormsRootDynamicViewModelPresenter()
        {
            IsClosed = DataConstant.Create<object>(typeof(XamarinFormsBootstrapperBase), nameof(IsClosed), false);
            IsRootConstant = DataConstant.Create<object>(typeof(XamarinFormsBootstrapperBase), nameof(IsRootConstant), false);
        }

        [Preserve(Conditional = true)]
        public XamarinFormsRootDynamicViewModelPresenter(IViewManager viewManager, INavigationDispatcher navigationDispatcher,
            IViewMappingProvider viewMappingProvider, IThreadManager threadManager, IEventAggregator eventAggregator)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(eventAggregator, nameof(eventAggregator));
            ViewManager = viewManager;
            NavigationDispatcher = navigationDispatcher;
            ViewMappingProvider = viewMappingProvider;
            ThreadManager = threadManager;
            eventAggregator.Subscribe(this);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected IThreadManager ThreadManager { get; }

        protected IViewManager ViewManager { get; }

        protected IViewMappingProvider ViewMappingProvider { get; }

        public int Priority => int.MaxValue - 1;

        public bool WrapToNavigationPage { get; set; }

        public Func<Page, NavigationPage> NavigationPageFactory { get; set; }

        #endregion

        #region Methods

        protected virtual void InitializeRootPage(IViewModel viewModel, IDataContext context)
        {
            var mainPage = (Page)ViewManager.GetOrCreateView(viewModel, true, context);
            var navigationPage = mainPage as NavigationPage;
            if (WrapToNavigationPage)
                navigationPage = CreateNavigationPage(mainPage);

            var isRoot = navigationPage == null || ReferenceEquals(mainPage, navigationPage);
            if (navigationPage != null)
            {
                INavigationService navigationService;
                if (!ServiceProvider.TryGet(out navigationService))
                {
                    navigationService = CreateNavigationService();
                    ServiceProvider.IocContainer.BindToConstant(navigationService);
                }

                //Activating navigation provider if need
                INavigationProvider provider;
                ServiceProvider.TryGet(out provider);

                navigationService.UpdateRootPage(navigationPage, viewModel);
                mainPage = navigationPage;
            }
            Application.Current.MainPage = mainPage;

            EventHandler<Page, CancelEventArgs> handler = OnBackButtonPressed;
            XamarinFormsToolkitExtensions.BackButtonPressed -= handler;

            var mode = NavigationMode.New;
            if (isRoot)
            {
                XamarinFormsToolkitExtensions.BackButtonPressed += handler;

                if (viewModel.Settings.State.Contains(IsRootConstant))
                    mode = NavigationMode.Refresh;
                else
                    viewModel.Settings.State.AddOrUpdate(IsRootConstant, null);
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.CanCloseHandler, CanCloseRootViewModel);
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, mode, null, viewModel, this, context));
            }
            else
            {
                mainPage.SetNavigationParameter(NavigationProvider.GenerateNavigationParameter(viewModel));
                ServiceProvider.Get<INavigationProvider>().Restore(context);
            }
        }

        [CanBeNull]
        protected virtual NavigationPage CreateNavigationPage(Page mainPage)
        {
            if (NavigationPageFactory == null)
                return new NavigationPage(mainPage);
            return NavigationPageFactory(mainPage);
        }

        [NotNull]
        protected virtual INavigationService CreateNavigationService()
        {
            return new NavigationService();
        }

        protected virtual void OnBackButtonPressed(Page sender, CancelEventArgs args)
        {
            var viewModel = sender.BindingContext as IViewModel;
            if (viewModel == null)
                return;
            var task = TryCloseRootAsync(viewModel, DataContext.Empty);
            args.Cancel = task != null && (!task.IsCompleted || !task.Result);
        }

        protected virtual void OnApplicationStart()
        {
            var openedViewModels = NavigationDispatcher.GetOpenedViewModels(NavigationType.Page);
            foreach (var openedViewModel in openedViewModels)
            {
                Tracer.Warn($"There is an open view model {openedViewModel} after app restart");
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Remove, openedViewModel.ViewModel, null, this));
            }
        }

        protected virtual bool CanCloseRootViewModel(IViewModel viewModel, object o)
        {
            return XamarinFormsToolkitExtensions.SendBackButtonPressed != null && viewModel.Settings.State.Contains(IsRootConstant);
        }

        private Task<bool> TryCloseRootAsync(IViewModel viewModel, IDataContext context)
        {
            if (viewModel == null || !viewModel.Settings.State.Contains(IsRootConstant) || viewModel.Settings.Metadata.Contains(IsClosed))
                return null;

            var currentView = viewModel.GetCurrentView<object>();

            var backButtonAction = XamarinFormsToolkitExtensions.SendBackButtonPressed?.Invoke(currentView);
            if (backButtonAction == null)
                return null;

            var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Back, viewModel, null, this, context);
            var task = NavigationDispatcher.OnNavigatingAsync(navigationContext);
            if (task.IsCompleted)
            {
                if (task.Result)
                {
                    viewModel.Settings.Metadata.AddOrUpdate(IsClosed, null);
                    _hasRootPage = false;
                    NavigationDispatcher.OnNavigated(navigationContext);
                }
                return task;
            }
            return task.TryExecuteSynchronously(t =>
            {
                if (!t.Result)
                    return false;
                _hasRootPage = false;
                viewModel.Settings.Metadata.AddOrUpdate(IsClosed, null);
                backButtonAction();
                NavigationDispatcher.OnNavigated(navigationContext);
                return true;
            });
        }

        #endregion

        #region Implementation of interfaces

        public virtual IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (context.GetData(NavigationConstants.SuppressRootNavigation))
                return null;

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null || _hasRootPage)
                return null;

            var mappingItem = ViewMappingProvider.FindMappingForViewModel(viewModel.GetType(), viewModel.GetViewName(context), false);
            if (mappingItem == null || !typeof(Page).IsAssignableFrom(mappingItem.ViewType))
                return null;

            _hasRootPage = true;
            var operation = viewModel.RegisterNavigationOperation(OperationType.PageNavigation, context);
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, context, (@this, model, arg3) => @this.InitializeRootPage(model, arg3));
            return operation;
        }

        public virtual Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            return TryCloseRootAsync(viewModel, context);
        }

        void IHandler<BackgroundNavigationMessage>.Handle(object sender, BackgroundNavigationMessage message)
        {
            var viewModel = Application.Current?.MainPage?.BindingContext as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Background, viewModel, null, this, message.Context));
        }

        void IHandler<ForegroundNavigationMessage>.Handle(object sender, ForegroundNavigationMessage message)
        {
            var viewModel = Application.Current?.MainPage?.BindingContext as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Foreground, null, viewModel, this, message.Context));
        }

        void IHandler<ApplicationStartingMessage>.Handle(object sender, ApplicationStartingMessage message)
        {
            _hasRootPage = false;
            OnApplicationStart();
        }

        #endregion        
    }
}