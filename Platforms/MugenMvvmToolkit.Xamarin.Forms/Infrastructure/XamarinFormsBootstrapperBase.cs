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
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Infrastructure
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

        protected static readonly DataConstant<bool> WrapToNavigationPageConstant;
        private readonly IPlatformService _platformService;
        private IViewModel _mainViewModel;
        private PlatformInfo _platform;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamarinFormsBootstrapperBase" /> class.
        /// </summary>
        static XamarinFormsBootstrapperBase()
        {
            WrapToNavigationPageConstant = DataConstant.Create(() => WrapToNavigationPageConstant);
            if (Device.OS != TargetPlatform.WinPhone)
                LinkerInclude.Initialize();
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
            ViewManager.ViewCleared += OnViewCleared;
            ViewManager.ClearDataContext = true;
            BindingServiceProvider.DataContextMemberAliases.Add("BindingContext");
            BindingServiceProvider.BindingMemberPriorities["BindingContext"] = int.MaxValue - 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamarinFormsBootstrapperBase" /> class.
        /// </summary>
        protected XamarinFormsBootstrapperBase()
        {
            Assembly assembly = TryLoadAssembly(BindingAssemblyName, null);
            if (assembly == null)
                return;
            TypeInfo serviceType = typeof(IPlatformService).GetTypeInfo();
            serviceType = assembly.DefinedTypes.FirstOrDefault(serviceType.IsAssignableFrom);
            if (serviceType != null)
            {
                _platformService = (IPlatformService)Activator.CreateInstance(serviceType.AsType());
                BindingServiceProvider.ValueConverter = _platformService.ValueConverter;
            }
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
                return Device.OnPlatform("MugenMvvmToolkit.Xamarin.Forms.iOS", "MugenMvvmToolkit.Xamarin.Forms.Android",
                    "MugenMvvmToolkit.Xamarin.Forms.WinPhone");
            }
        }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public override PlatformInfo Platform
        {
            get
            {
                if (_platform == null)
                    _platform = _platformService == null
                        ? XamarinFormsExtensions.GetPlatformInfo()
                        : _platformService.GetPlatformInfo();
                return _platform;
            }
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            if (_platformService == null)
                return base.GetAssemblies();
            return _platformService.GetAssemblies();
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

            InitializationContext = InitializationContext.ToNonReadOnly();
            InitializationContext.AddOrUpdate(WrapToNavigationPageConstant, wrapToNavigationPage);
            if (_mainViewModel == null)
            {
                Initialize();
                Type viewModelType = GetMainViewModelType();
                _mainViewModel = CreateMainViewModel(viewModelType);
            }

            var view = (Page)ViewManager.GetOrCreateView(_mainViewModel, true, new DataContext(InitializationContext));
            NavigationPage page = view as NavigationPage ?? CreateNavigationPage(view);
            if (page == null)
                return view;
            INavigationService navigationService;
            if (!IocContainer.TryGet(out navigationService))
            {
                navigationService = CreateNavigationService();
                IocContainer.BindToConstant(navigationService);
            }
            //Activating navigation provider
            INavigationProvider provider;
            IocContainer.TryGet(out provider);

            navigationService.UpdateRootPage(page);
            return page;
        }

        /// <summary>
        ///     Creates the main view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel CreateMainViewModel([NotNull] Type viewModelType)
        {
            return IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(viewModelType, InitializationContext);
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        /// <summary>
        ///     Creates an instance of <see cref="NavigationPage" />
        /// </summary>
        [CanBeNull]
        protected virtual NavigationPage CreateNavigationPage(Page mainPage)
        {
            if (InitializationContext.GetData(WrapToNavigationPageConstant))
                return new NavigationPage(mainPage);
            return null;
        }

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />
        /// </summary>
        [NotNull]
        protected virtual INavigationService CreateNavigationService()
        {
            return new NavigationService(IocContainer.Get<IThreadManager>());
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