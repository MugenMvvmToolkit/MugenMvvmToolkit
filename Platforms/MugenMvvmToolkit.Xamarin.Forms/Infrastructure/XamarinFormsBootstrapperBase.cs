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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Presenters;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
{
    public abstract class XamarinFormsBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Nested types

        public interface IPlatformService
        {
            PlatformInfo GetPlatformInfo();

            ICollection<Assembly> GetAssemblies();
        }

        #endregion

        #region Fields

        private WeakReference _mainViewModelRef;
        private readonly PlatformInfo _platform;
        private readonly IPlatformService _platformService;
        private bool _wrapToNavigationPage;

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
        }

        #endregion

        #region Properties

        [CanBeNull]
        public new static XamarinFormsBootstrapperBase Current => BootstrapperBase.Current as XamarinFormsBootstrapperBase;

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

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            _mainViewModelRef = ServiceProvider.WeakReferenceFactory(viewModel);

            var view = (Page)ServiceProvider.ViewManager.GetOrCreateView(viewModel, true, context);
            NavigationPage page = view as NavigationPage;
            if (page == null && _wrapToNavigationPage)
                page = CreateNavigationPage(view);
            if (page != null)
            {
                var iocContainer = ServiceProvider.IocContainer;
                INavigationService navigationService;
                if (!iocContainer.TryGet(out navigationService))
                {
                    navigationService = CreateNavigationService();
                    iocContainer.BindToConstant(navigationService);
                }
                //Activating navigation provider
                INavigationProvider provider;
                iocContainer.TryGet(out provider);

                navigationService.UpdateRootPage(page);
                view = page;
            }
            Application.Current.MainPage = view;
            return new AsyncOperation<object>();
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion

        #region Methods

        public virtual void Start(bool wrapToNavigationPage = true, IDataContext context = null)
        {
            if (Current != null && !ReferenceEquals(Current, this))
            {
                Current.Start(wrapToNavigationPage, context);
                return;
            }
            _wrapToNavigationPage = wrapToNavigationPage;
            Initialize();
            var app = ServiceProvider.Application;
            var viewModelPresenter = app.IocContainer.Get<IViewModelPresenter>();
            viewModelPresenter.DynamicPresenters.Add(this);

            var viewModel = _mainViewModelRef?.Target as IViewModel;
            if (viewModel == null || viewModel.IsDisposed)
            {
                var presenter = viewModelPresenter as IRestorableViewModelPresenter;
                if (presenter == null || !presenter.TryRestore(context))
                    app.Start(context);
            }
            else
                viewModel.ShowAsync(context);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            return _platformService.GetAssemblies().Where(x => !x.IsDynamic).ToList();
        }

        [CanBeNull]
        protected virtual NavigationPage CreateNavigationPage(Page mainPage)
        {
            return new NavigationPage(mainPage);
        }

        [NotNull]
        protected virtual INavigationService CreateNavigationService()
        {
            return new NavigationService(ServiceProvider.ThreadManager, true);
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
