#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsBootstrapperBase.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
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
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Presenters;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
{
    public abstract class XamarinFormsBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter, IHandler<BackgroundNavigationMessage>, IHandler<ForegroundNavigationMessage>//todo dynamic presenter
    {
        #region Nested types

        public interface IPlatformService
        {
            PlatformInfo GetPlatformInfo();

            ICollection<Assembly> GetAssemblies();

            void Initialize();
        }

        #endregion

        #region Fields

        protected static readonly DataConstant<object> IsRootConstant = DataConstant.Create<object>(typeof(XamarinFormsBootstrapperBase), nameof(IsRootConstant), false);
        private readonly PlatformInfo _platform;
        private readonly IPlatformService _platformService;
        private bool _hasRootPage;

        #endregion

        #region Constructors

        static XamarinFormsBootstrapperBase()
        {
            SetDefaultPlatformValues();
        }

        protected XamarinFormsBootstrapperBase(IPlatformService platformService)
        {
            Should.NotBeNull(platformService, nameof(platformService));
            _platformService = platformService;
            _platform = platformService.GetPlatformInfo();
            WrapToNavigationPage = true;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public new static XamarinFormsBootstrapperBase Current => BootstrapperBase.Current as XamarinFormsBootstrapperBase;

        public bool WrapToNavigationPage { get; set; }

        protected internal static string BindingAssemblyName
        {
            get
            {
                if (Device.OS == TargetPlatform.Windows)
                    return "MugenMvvmToolkit.Xamarin.Forms.UWP";
                return Device.OnPlatform("MugenMvvmToolkit.Xamarin.Forms.iOS", "MugenMvvmToolkit.Xamarin.Forms.Android", "MugenMvvmToolkit.Xamarin.Forms.WinPhone");
            }
        }

        #endregion

        #region Overrides of BootstrapperBase

        protected override PlatformInfo Platform => _platform;

        protected override IList<Assembly> GetAssemblies()
        {
            return _platformService.GetAssemblies().Where(x => !x.IsDynamic).ToList();
        }

        #endregion

        #region Implementation of interfaces

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null || _hasRootPage)
                return null;

            var mappingItem = ServiceProvider.Get<IViewMappingProvider>().FindMappingForViewModel(viewModel.GetType(), viewModel.GetViewName(context), false);
            if (mappingItem == null || !typeof(Page).IsAssignableFrom(mappingItem.ViewType))
                return null;

            _hasRootPage = true;
            var operation = new AsyncOperation<object>();
            ServiceProvider.Get<IOperationCallbackManager>().Register(OperationType.PageNavigation, viewModel, operation.ToOperationCallback(), context);
            ServiceProvider.ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, context, (@this, model, arg3) => @this.InitializeRootPage(model, arg3));
            return operation;
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return null;
            if (viewModel.Settings.State.Contains(IsRootConstant))
            {
                var navigationDispatcher = ServiceProvider.Get<INavigationDispatcher>();
                var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModel, null, this, context);
                return navigationDispatcher
                    .OnNavigatingAsync(navigationContext)
                    .TryExecuteSynchronously(task =>
                    {
                        if (task.Result)
                        {
                            var page = Application.Current.MainPage;
                            _hasRootPage = false;
                            ServiceProvider.ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, navigationDispatcher, page, navigationContext, (d, p, ctx) =>
                            {
                                if (ReferenceEquals(Application.Current.MainPage, p))
                                    Application.Current.MainPage = new Page();
                                d.OnNavigated(ctx);
                            });
                        }
                        return task.Result;
                    });
            }
            return null;
        }

        void IHandler<BackgroundNavigationMessage>.Handle(object sender, BackgroundNavigationMessage message)
        {
            var viewModel = Application.Current.MainPage?.DataContext() as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                ServiceProvider.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Background, viewModel, null, this, message.Context));
        }

        void IHandler<ForegroundNavigationMessage>.Handle(object sender, ForegroundNavigationMessage message)
        {
            var viewModel = Application.Current.MainPage?.DataContext() as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                ServiceProvider.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Foreground, null, viewModel, this, message.Context));
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            _platformService.Initialize();
            ServiceProvider.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            XamarinFormsExtensions.BackButtonPressed += OnBackButtonPressed;
        }

        public virtual void Start()
        {
            if (Current != null && !ReferenceEquals(Current, this))
            {
                Current.Start();
                return;
            }
            Initialize();
            OnStart();

            var app = ServiceProvider.Application;
            var context = new DataContext(app.Context);
            _hasRootPage = false;

            var viewModelPresenter = app.IocContainer.Get<IViewModelPresenter>();
            var presenter = viewModelPresenter as IRestorableViewModelPresenter;
            if (presenter == null || !presenter.TryRestore(context))
                app.Start();
        }

        protected virtual void InitializeRootPage(IViewModel viewModel, IDataContext context)
        {
            var mainPage = (Page)ServiceProvider.ViewManager.GetOrCreateView(viewModel, true, context);
            mainPage.SetNavigationParameter(NavigationProvider.GenerateNavigationParameter(viewModel.GetType()));
            NavigationPage navigationPage = mainPage as NavigationPage;
            if (WrapToNavigationPage)
                navigationPage = CreateNavigationPage(mainPage);

            bool isRoot = ReferenceEquals(mainPage, navigationPage);
            if (navigationPage != null)
            {
                var iocContainer = ServiceProvider.IocContainer;
                INavigationService navigationService;
                if (!iocContainer.TryGet(out navigationService))
                {
                    navigationService = CreateNavigationService();
                    iocContainer.BindToConstant(navigationService);
                }

                //Activating navigation provider if need
                INavigationProvider provider;
                iocContainer.TryGet(out provider);

                navigationService.UpdateRootPage(navigationPage);
                mainPage = navigationPage;
            }
            Application.Current.MainPage = mainPage;

            NavigationMode mode = NavigationMode.New;
            if (isRoot)
            {
                if (viewModel.Settings.State.Contains(IsRootConstant))
                    mode = NavigationMode.Refresh;
                viewModel.Settings.State.AddOrUpdate(IsRootConstant, null);
            }
            ServiceProvider.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, mode, null, viewModel, this, context));
        }

        [CanBeNull]
        protected virtual NavigationPage CreateNavigationPage(Page mainPage)
        {
            return new NavigationPage(mainPage);
        }

        [NotNull]
        protected virtual INavigationService CreateNavigationService()
        {
            return new NavigationService();
        }

        protected virtual void OnStart()
        {
            var navigationDispatcher = ServiceProvider.Get<INavigationDispatcher>();
            var openedViewModels = navigationDispatcher.GetOpenedViewModels(NavigationType.Page);
            foreach (var openedViewModel in openedViewModels)
            {
                Tracer.Warn($"There is an open view model {openedViewModel} after app restart");
                navigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Remove, openedViewModel, null, this));
            }
        }

        private static void OnBackButtonPressed(Page sender, CancelEventArgs args)
        {
            var viewModel = sender.DataContext() as IViewModel;
            if (viewModel == null || !viewModel.Settings.State.Contains(IsRootConstant))
                return;

            var backButtonAction = XamarinFormsExtensions.SendBackButtonPressed?.Invoke(sender);
            if (backButtonAction == null)
                return;

            var navigationDispatcher = ServiceProvider.Get<INavigationDispatcher>();
            var context = new NavigationContext(NavigationType.Page, NavigationMode.Back, viewModel, null, Current);
            var task = navigationDispatcher.OnNavigatingAsync(context);
            if (task.IsCompleted)
            {
                args.Cancel = !task.Result;
                if (task.Result)
                    navigationDispatcher.OnNavigated(context);
            }
            else
            {
                task.TryExecuteSynchronously(t =>
                {
                    if (!t.Result)
                        return;
                    backButtonAction();
                    navigationDispatcher.OnNavigated(context);
                });
            }
        }

        internal static void SetDefaultPlatformValues()
        {
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
            ApplicationSettings.ViewManagerClearDataContext = true;
            BindingServiceProvider.DataContextMemberAliases.Add(nameof(BindableObject.BindingContext));
            BindingServiceProvider.BindingMemberPriorities[nameof(BindableObject.BindingContext)] = BindingServiceProvider.DataContextMemberPriority;
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            string viewName = viewModel.GetViewName(dataContext);
            IIocContainer container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            IViewMappingItem mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null ||
                   typeof(ITabView).GetTypeInfo().IsAssignableFrom(mappingItem.ViewType.GetTypeInfo()) ||
                   !typeof(Page).GetTypeInfo().IsAssignableFrom(mappingItem.ViewType.GetTypeInfo());
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext,
            IViewModelPresenter arg3)
        {
            string viewName = viewModel.GetViewName(dataContext);
            IIocContainer container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            IViewMappingItem mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null &&
                   typeof(Page).GetTypeInfo().IsAssignableFrom(mappingItem.ViewType.GetTypeInfo());
        }

        #endregion
    }
}
