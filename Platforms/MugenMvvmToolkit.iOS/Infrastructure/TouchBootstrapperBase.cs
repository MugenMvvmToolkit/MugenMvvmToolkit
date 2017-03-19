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
    public abstract class TouchBootstrapperBase : BootstrapperBase, IRestorableDynamicViewModelPresenter
    {
        #region Fields

        private static readonly DataConstant<object> IsRootPage = DataConstant.Create<object>(typeof(TouchBootstrapperBase), nameof(IsRootPage), false);
        private readonly UIWindow _window;
        private readonly PlatformInfo _platform;
        private bool _isStarted;

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
            if (!_isStarted)
            {
                parentPresenter.DynamicPresenters.Remove(this);
                return null;
            }

            if (WrapToNavigationController)
            {
                parentPresenter.DynamicPresenters.Remove(this);
                return parentPresenter.ShowAsync(context);
            }

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return null;
            var mappingItem = ServiceProvider.Get<IViewMappingProvider>().FindMappingForViewModel(viewModel.GetType(), viewModel.GetViewName(context), false);
            if (mappingItem == null || !typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType))
                return null;

            parentPresenter.DynamicPresenters.Remove(this);
            _window.RootViewController = (UIViewController)ServiceProvider.ViewManager.GetOrCreateView(viewModel, null, context);
            ServiceProvider.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Window, NavigationMode.New, null, viewModel, this, context));
            viewModel.Settings.State.AddOrUpdate(IsRootPage, null);
            return new AsyncOperation<object>();
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        bool IRestorableDynamicViewModelPresenter.Restore(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (viewModel.Settings.State.Contains(IsRootPage))
            {
                parentPresenter.DynamicPresenters.Remove(this);
                ServiceProvider.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Window, NavigationMode.Refresh, null, viewModel, this, context));
                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            ServiceProvider.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            var navigationService = CreateNavigationService(_window);
            if (navigationService != null)
                ServiceProvider.IocContainer.BindToConstant(navigationService);
        }

        public virtual void Start()
        {
            Initialize();
            _isStarted = true;
            ServiceProvider.Application.Start();
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(UIWindow window)
        {
            if (WrapToNavigationController)
                return new NavigationService(window);
            return null;
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
