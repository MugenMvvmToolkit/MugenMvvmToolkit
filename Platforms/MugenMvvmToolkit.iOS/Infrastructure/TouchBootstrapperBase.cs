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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Infrastructure.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Navigation;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
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
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
            ServiceProvider.WeakReferenceFactory = PlatformExtensions.CreateWeakReference;
            ApplicationSettings.ViewManagerDisposeView = true;
            BindingServiceProvider.CompiledExpressionInvokerSupportCoalesceExpression = false;
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

        protected override PlatformInfo Platform => _platform;

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            if (WrapToNavigationController)
                return parentPresenter.ShowAsync(context);

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return null;

            _window.RootViewController = (UIViewController)ServiceProvider.ViewManager.GetOrCreateView(viewModel, null, context);
            ServiceProvider.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Window, NavigationMode.New, null, viewModel, this, context));
            return new AsyncOperation<object>();
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            _navigationService = CreateNavigationService(_window);
            if (_navigationService != null)
                ServiceProvider.Application.IocContainer.BindToConstant(_navigationService);
        }

        public virtual void Start()
        {
            Initialize();
            var app = ServiceProvider.Application;
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            app.Start();
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(UIWindow window)
        {
            return new NavigationService(window);
        }

        protected override IList<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).ToList();
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
