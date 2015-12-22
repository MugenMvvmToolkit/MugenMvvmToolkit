#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsBootstrapperBase.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Presenters;
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
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Views;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
{
    public abstract class XamarinFormsBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Nested types

        internal interface IPlatformService
        {
            Func<IBindingMemberInfo, Type, object, object> ValueConverter { get; }

            PlatformInfo GetPlatformInfo();

            ICollection<Assembly> GetAssemblies();
        }

        #endregion

        #region Fields

        private const string WinRTAssemblyName = "MugenMvvmToolkit.Xamarin.Forms.WinRT";
        private static IPlatformService _platformService;
        private readonly PlatformInfo _platform;
        private IViewModel _mainViewModel;
        private bool _wrapToNavigationPage;

        #endregion

        #region Constructors

        static XamarinFormsBootstrapperBase()
        {
            MvvmApplication.SetDefaultDesignTimeManager();
            if (Device.OS != TargetPlatform.WinPhone)
                LinkerInclude.Initialize();
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
            ViewManager.ViewCleared += OnViewCleared;
            ViewManager.ClearDataContext = true;
            var contextName = ToolkitExtensions.GetMemberName<BindableObject>(() => e => e.BindingContext);
            BindingServiceProvider.DataContextMemberAliases.Add(contextName);
            BindingServiceProvider.BindingMemberPriorities[contextName] = BindingServiceProvider.DataContextMemberPriority;
        }

        protected XamarinFormsBootstrapperBase(PlatformInfo platform = null)
        {
            _platform = platform ?? GetPlatformInfo();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public new static XamarinFormsBootstrapperBase Current => BootstrapperBase.Current as XamarinFormsBootstrapperBase;

        protected static string BindingAssemblyName
        {
            get
            {
                if (Device.OS == TargetPlatform.Windows)
                    return WinRTAssemblyName;
                return Device.OnPlatform("MugenMvvmToolkit.Xamarin.Forms.iOS", "MugenMvvmToolkit.Xamarin.Forms.Android",
                    "MugenMvvmToolkit.Xamarin.Forms.WinPhone");
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

        INavigationOperation IDynamicViewModelPresenter.TryShowAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            _mainViewModel = viewModel;

            var view = (Page)ViewManager.GetOrCreateView(_mainViewModel, true, context);
            NavigationPage page = view as NavigationPage;
            if (page == null && _wrapToNavigationPage)
                page = CreateNavigationPage(view);
            if (page != null)
            {
                var iocContainer = MvvmApplication.Current.IocContainer;
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
            return new NavigationOperation();
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
            var app = MvvmApplication.Current;
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);

            if (_mainViewModel == null || _mainViewModel.IsDisposed)
                app.Start(context);
            else
                _mainViewModel.ShowAsync(context);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            if (_platformService == null)
                return new[] { GetType().GetTypeInfo().Assembly, typeof(BootstrapperBase).GetTypeInfo().Assembly };
            return _platformService.GetAssemblies();
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

        private static PlatformInfo GetPlatformInfo()
        {
            Assembly assembly = TryLoadAssembly(BindingAssemblyName, null);
            if (assembly == null)
            {
                if (Device.OS == TargetPlatform.WinPhone)
                    assembly = TryLoadAssembly(WinRTAssemblyName, null);
                if (assembly == null)
                    return XamarinFormsExtensions.GetPlatformInfo();
            }
            TypeInfo serviceType = typeof(IPlatformService).GetTypeInfo();
            serviceType = assembly.DefinedTypes.FirstOrDefault(serviceType.IsAssignableFrom);
            if (serviceType != null)
            {
                _platformService = (IPlatformService)Activator.CreateInstance(serviceType.AsType());
                BindingServiceProvider.ValueConverter = _platformService.ValueConverter;
            }
            return _platformService == null
                ? XamarinFormsExtensions.GetPlatformInfo()
                : _platformService.GetPlatformInfo();
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

        private static void OnViewCleared(IViewManager viewManager, IViewModel viewModel, object arg3, IDataContext arg4)
        {
            try
            {
                XamarinFormsExtensions.ClearBindingsRecursively(arg3 as BindableObject, true, true);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        #endregion
    }
}
