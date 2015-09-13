#region Copyright

// ****************************************************************************
// <copyright file="TouchBootstrapperBase.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Infrastructure.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class TouchBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private readonly UIWindow _window;
        private readonly PlatformInfo _platform;
        private INavigationService _navigationService;

        #endregion

        #region Constructors

        static TouchBootstrapperBase()
        {
            LinkerInclude.Initialize();
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
            ServiceProvider.WeakReferenceFactory = PlatformExtensions.CreateWeakReference;
            ViewManager.DisposeView = true;
            CompiledExpressionInvoker.SupportCoalesceExpression = false;
            BindingServiceProvider.ValueConverter = BindingReflectionExtensions.Convert;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TouchBootstrapperBase" /> class.
        /// </summary>
        protected TouchBootstrapperBase([NotNull] UIWindow window, PlatformInfo platform = null)
        {
            Should.NotBeNull(window, "window");
            _window = window;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            WrapToNavigationController = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current app window.
        /// </summary>
        protected UIWindow Window
        {
            get { return _window; }
        }

        /// <summary>
        ///     Indicates that bootstrapper should wraps the main controller to the navigation controller, default is <c>true</c>.
        /// </summary>
        public bool WrapToNavigationController { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
            _navigationService = CreateNavigationService(_window);
            if (_navigationService != null)
                iocContainer.BindToConstant(_navigationService);
        }

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            Initialize();
            var app = MvvmApplication.Current;
            var ctx = new DataContext(app.Context);
            var viewModelType = app.GetStartViewModelType();
            var viewModel = app.IocContainer
               .Get<IViewModelProvider>()
               .GetViewModel(viewModelType, ctx);
            if (WrapToNavigationController)
                viewModel.ShowAsync((model, result) => model.Dispose(), null, ctx);
            else
                _window.RootViewController = (UIViewController)ViewManager.GetOrCreateView(viewModel, null, ctx);
        }

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />.
        /// </summary>
        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(UIWindow window)
        {
            return new NavigationService(window);
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            return new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies());
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || typeof(ITabView).IsAssignableFrom(mappingItem.ViewType) ||
                   !typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion
    }
}