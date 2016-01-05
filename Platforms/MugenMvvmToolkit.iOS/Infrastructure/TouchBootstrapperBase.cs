#region Copyright

// ****************************************************************************
// <copyright file="TouchBootstrapperBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure
{
    public abstract class TouchBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
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

        protected TouchBootstrapperBase([NotNull] UIWindow window, PlatformInfo platform = null)
        {
            Should.NotBeNull(window, nameof(window));
            _window = window;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            WrapToNavigationController = true;
        }

        #endregion

        #region Properties

        protected UIWindow Window => _window;

        public bool WrapToNavigationController { get; set; }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        INavigationOperation IDynamicViewModelPresenter.TryShowAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            if (WrapToNavigationController)
                return parentPresenter.ShowAsync(viewModel, context);
            _window.RootViewController = (UIViewController)ViewManager.GetOrCreateView(viewModel, null, context);
            return new NavigationOperation();
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
            _navigationService = CreateNavigationService(_window);
            if (_navigationService != null)
                iocContainer.BindToConstant(_navigationService);
        }

        public virtual void Start(IDataContext context = null)
        {
            Initialize();
            var app = MvvmApplication.Current;
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            app.Start(context);
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(UIWindow window)
        {
            return new NavigationService(window);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
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
