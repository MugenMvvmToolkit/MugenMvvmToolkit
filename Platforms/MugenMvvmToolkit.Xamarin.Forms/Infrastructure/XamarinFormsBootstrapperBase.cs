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
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
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
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class XamarinFormsBootstrapperBase : BootstrapperBase
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

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamarinFormsBootstrapperBase" /> class.
        /// </summary>
        static XamarinFormsBootstrapperBase()
        {
            ServiceProvider.SetDefaultDesignTimeManager();
            if (Device.OS != TargetPlatform.WinPhone)
                LinkerInclude.Initialize();
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
            ViewManager.ViewCleared += OnViewCleared;
            ViewManager.ClearDataContext = true;
            var contextName = ToolkitExtensions.GetMemberName<BindableObject>(() => e => e.BindingContext);
            BindingServiceProvider.DataContextMemberAliases.Add(contextName);
            BindingServiceProvider.BindingMemberPriorities[contextName] = int.MaxValue - 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamarinFormsBootstrapperBase" /> class.
        /// </summary>
        protected XamarinFormsBootstrapperBase(PlatformInfo platform = null)
        {
            _platform = platform ?? GetPlatformInfo();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the current <see cref="XamarinFormsBootstrapperBase" />.
        /// </summary>
        [CanBeNull]
        public new static XamarinFormsBootstrapperBase Current
        {
            get { return BootstrapperBase.Current as XamarinFormsBootstrapperBase; }
        }

        /// <summary>
        ///     Gets the name of binding assembly.
        /// </summary>
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

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual Page Start(bool wrapToNavigationPage = true)
        {
            if (Current != null && !ReferenceEquals(Current, this))
                return Current.Start(wrapToNavigationPage);
            Initialize();
            var app = MvvmApplication.Current;
            var iocContainer = app.IocContainer;
            var ctx = new DataContext(app.Context);
            if (_mainViewModel == null || _mainViewModel.IsDisposed)
            {
                Type viewModelType = app.GetStartViewModelType();
                _mainViewModel = iocContainer
                    .Get<IViewModelProvider>()
                    .GetViewModel(viewModelType, ctx);
            }

            var view = (Page)ViewManager.GetOrCreateView(_mainViewModel, true, ctx);
            NavigationPage page = view as NavigationPage;
            if (page == null && wrapToNavigationPage)
                page = CreateNavigationPage(view);
            if (page == null)
                return view;
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
            return page;
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            if (_platformService == null)
                return new[] { GetType().GetTypeInfo().Assembly, typeof(BootstrapperBase).GetTypeInfo().Assembly };
            return _platformService.GetAssemblies();
        }

        /// <summary>
        ///     Creates an instance of <see cref="NavigationPage" />
        /// </summary>
        [CanBeNull]
        protected virtual NavigationPage CreateNavigationPage(Page mainPage)
        {
            return new NavigationPage(mainPage);
        }

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />
        /// </summary>
        [NotNull]
        protected virtual INavigationService CreateNavigationService()
        {
            return new NavigationService(ServiceProvider.ThreadManager);
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
            XamarinFormsExtensions.ClearBindingsRecursively(arg3 as BindableObject, true, true);
        }

        #endregion
    }
}