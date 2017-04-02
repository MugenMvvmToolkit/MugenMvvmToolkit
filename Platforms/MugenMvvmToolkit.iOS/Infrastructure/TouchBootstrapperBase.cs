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

using System.Collections.Generic;
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
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure
{
    public abstract class TouchBootstrapperBase : BootstrapperBase, IRestorableDynamicViewModelPresenter, IHandler<BackgroundNavigationMessage>, IHandler<ForegroundNavigationMessage>
    {
        #region Fields

        protected static readonly DataConstant<object> IsRootConstant = DataConstant.Create<object>(typeof(TouchBootstrapperBase), nameof(IsRootConstant), false);
        private readonly UIWindow _window;
        private readonly PlatformInfo _platform;
        private bool _isStarted;
        private bool _hasRootPage;
        private bool _skipForeground;

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
            _platform = platform ?? TouchToolkitExtensions.GetPlatformInfo();
        }

        protected TouchBootstrapperBase([NotNull] UIWindow window, PlatformInfo platform = null)
            : this(false, platform)
        {
            Should.NotBeNull(window, nameof(window));
            _window = window;
            WrapToNavigationController = true;
            _skipForeground = true;
        }

        #endregion

        #region Properties

        protected UIWindow Window => _window;

        public bool WrapToNavigationController { get; set; }

        protected override PlatformInfo Platform => _platform;

        #endregion

        #region Implementation of interfaces

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (!_isStarted || _hasRootPage)
                return null;

            if (WrapToNavigationController)
            {
                _hasRootPage = true;
                return parentPresenter.ShowAsync(context);
            }

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return null;

            var mappingItem = IocContainer.Get<IViewMappingProvider>().FindMappingForViewModel(viewModel.GetType(), viewModel.GetViewName(context), false);
            if (mappingItem == null || !typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType))
                return null;

            _hasRootPage = true;
            viewModel.Settings.State.AddOrUpdate(IsRootConstant, null);
            var operation = new AsyncOperation<object>();
            IocContainer.Get<IOperationCallbackManager>().Register(OperationType.PageNavigation, viewModel, operation.ToOperationCallback(), context);
            ServiceProvider.ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, context, (@this, vm, ctx) => @this.InitializeRootPage(vm, ctx));
            return operation;
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return null;
            if (viewModel.Settings.State.Contains(IsRootConstant))
            {
                var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModel, null, this, context);
                return IocContainer
                    .Get<INavigationDispatcher>()
                    .OnNavigatingAsync(navigationContext)
                    .TryExecuteSynchronously(task =>
                    {
                        if (task.Result)
                        {
                            var controller = _window.RootViewController;
                            _hasRootPage = false;
                            ServiceProvider.ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, _window, controller, navigationContext, (w, c, ctx) =>
                            {
                                if (ReferenceEquals(w.RootViewController, c))
                                    w.RootViewController = null;
                                IocContainer.Get<INavigationDispatcher>().OnNavigated(ctx);
                            });
                        }
                        return task.Result;
                    });
            }
            return null;
        }

        bool IRestorableDynamicViewModelPresenter.Restore(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (viewModel.Settings.State.Contains(IsRootConstant))
            {
                _hasRootPage = true;
                _isStarted = true;
                IocContainer.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Refresh, null, viewModel, this, context));
                return true;
            }
            return false;
        }

        void IHandler<BackgroundNavigationMessage>.Handle(object sender, BackgroundNavigationMessage message)
        {
            _skipForeground = false;
            var viewModel = _window.RootViewController?.DataContext() as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                IocContainer.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Background, viewModel, null, this, message.Context));
        }

        void IHandler<ForegroundNavigationMessage>.Handle(object sender, ForegroundNavigationMessage message)
        {
            if (_skipForeground)
            {
                _skipForeground = false;
                return;
            }
            var viewModel = _window.RootViewController?.DataContext() as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                IocContainer.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Foreground, null, viewModel, this, message.Context));
        }

        #endregion

        #region Methods

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            ServiceProvider.EventAggregator.Subscribe(this);
            IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            var navigationService = CreateNavigationService(_window);
            if (navigationService != null)
                IocContainer.BindToConstant(navigationService);
        }

        public virtual void Start()
        {
            Initialize();
            _isStarted = true;
            MvvmApplication.Start();
        }

        protected virtual void InitializeRootPage(IViewModel viewModel, IDataContext context)
        {
            Window.RootViewController = (UIViewController)ServiceProvider.ViewManager.GetOrCreateView(viewModel, null, context);
            IocContainer.Get<INavigationDispatcher>().OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.New, null, viewModel, this, context));
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(UIWindow window)
        {
            if (WrapToNavigationController)
                return new NavigationService(window);
            return null;
        }

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            assemblies.Add(typeof(BindingServiceProvider).Assembly);
            assemblies.Add(typeof(TouchBootstrapperBase).Assembly);
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
