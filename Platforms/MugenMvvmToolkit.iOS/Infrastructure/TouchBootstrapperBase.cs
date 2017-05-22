#region Copyright

// ****************************************************************************
// <copyright file="TouchBootstrapperBase.cs">
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
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.iOS.Infrastructure.Navigation;
using MugenMvvmToolkit.iOS.Infrastructure.Presenters;
using MugenMvvmToolkit.iOS.Interfaces.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure
{
    public abstract class TouchBootstrapperBase : BootstrapperBase
    {
        #region Fields

        // ReSharper disable NotAccessedField.Local
        private static NSObject _backgroundObserver;
        private static NSObject _foregroundObserver;
        // ReSharper restore NotAccessedField.Local

        #endregion

        #region Constructors

        static TouchBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
            ServiceProvider.WeakReferenceFactory = TouchToolkitExtensions.CreateWeakReference;
            ApplicationSettings.ViewManagerDisposeView = true;
            BindingServiceProvider.CompiledExpressionInvokerSupportCoalesceExpression = false;
        }

        protected TouchBootstrapperBase(bool isDesignMode, PlatformInfo platform = null) : base(isDesignMode)
        {
            Platform = platform ?? TouchToolkitExtensions.GetPlatformInfo();
        }

        protected TouchBootstrapperBase([NotNull] UIWindow window, PlatformInfo platform = null)
            : this(false, platform)
        {
            Should.NotBeNull(window, nameof(window));
            Window = window;
            WrapToNavigationController = true;
        }

        #endregion

        #region Properties

        protected UIWindow Window { get; }

        public bool WrapToNavigationController { get; set; }

        public Func<IIocContainer, IDynamicViewModelPresenter> RootPresenterFactory { get; set; }

        protected override PlatformInfo Platform { get; }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            if (WrapToNavigationController)
            {
                var result = CreateNavigationService();
                if (result != null)
                    IocContainer.BindToConstant(result);
            }

            var rootPresenter = GetRootPresenter();
            if (rootPresenter != null)
                IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(rootPresenter);
            _backgroundObserver = UIApplication.Notifications.ObserveDidEnterBackground(OnApplicationDidEnterBackground);
            _foregroundObserver = UIApplication.Notifications.ObserveDidBecomeActive(OnApplicationDidBecomeActive);
        }

        public virtual void Start()
        {
            Initialize();
            MvvmApplication.Start();
        }

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic));
        }

        protected virtual IDynamicViewModelPresenter GetRootPresenter()
        {
            if (RootPresenterFactory != null)
                return RootPresenterFactory(IocContainer);
            if (WrapToNavigationController)
                return null;
            var presenter = IocContainer.Get<TouchRootDynamicViewModelPresenter>();
            presenter.Window = Window;
            return presenter;
        }

        protected virtual INavigationService CreateNavigationService()
        {
            return new NavigationService(Window);
        }

        private static void OnApplicationDidBecomeActive(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            ServiceProvider.Application?.SetApplicationState(ApplicationState.Active, null);
        }

        private static void OnApplicationDidEnterBackground(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            ServiceProvider.Application?.SetApplicationState(ApplicationState.Background, null);
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