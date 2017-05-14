#region Copyright

// ****************************************************************************
// <copyright file="TouchRootDynamicViewModelPresenter.cs">
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

using System.Threading.Tasks;
using Foundation;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.ViewModels;
using UIKit;

namespace MugenMvvmToolkit.iOS.Infrastructure.Presenters
{
    public class TouchRootDynamicViewModelPresenter : IRestorableDynamicViewModelPresenter, IHandler<BackgroundNavigationMessage>,
        IHandler<ForegroundNavigationMessage>
    {
        #region Fields

        private bool _hasRootPage;
        private bool _subscribed;

        protected static readonly DataConstant<object> IsRootConstant;

        #endregion

        #region Constructors

        static TouchRootDynamicViewModelPresenter()
        {
            IsRootConstant = DataConstant.Create<object>(typeof(TouchBootstrapperBase), nameof(IsRootConstant), false);
        }

        [Preserve(Conditional = true)]
        public TouchRootDynamicViewModelPresenter(IViewManager viewManager, INavigationDispatcher navigationDispatcher, IViewMappingProvider viewMappingProvider,
            IOperationCallbackManager operationCallbackManager, IThreadManager threadManager, IEventAggregator eventAggregator)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            Should.NotBeNull(operationCallbackManager, nameof(operationCallbackManager));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(eventAggregator, nameof(eventAggregator));
            ViewManager = viewManager;
            NavigationDispatcher = navigationDispatcher;
            ViewMappingProvider = viewMappingProvider;
            OperationCallbackManager = operationCallbackManager;
            ThreadManager = threadManager;
            EventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected IOperationCallbackManager OperationCallbackManager { get; }

        protected IThreadManager ThreadManager { get; }

        protected IEventAggregator EventAggregator { get; }

        protected IViewManager ViewManager { get; }

        protected IViewMappingProvider ViewMappingProvider { get; }

        public UIWindow Window { get; set; }

        public bool WrapToNavigationController { get; set; }

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        #endregion

        #region Methods

        protected virtual void InitializeRootPage(IViewModel viewModel, IDataContext context)
        {
            Window.RootViewController = (UIViewController)ViewManager.GetOrCreateView(viewModel, null, context);
            NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.New, null, viewModel, this, context));
        }

        #endregion

        #region Implementation of interfaces

        void IHandler<BackgroundNavigationMessage>.Handle(object sender, BackgroundNavigationMessage message)
        {
            var viewModel = Window?.RootViewController?.DataContext() as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Background, viewModel, null, this, message.Context));
        }

        void IHandler<ForegroundNavigationMessage>.Handle(object sender, ForegroundNavigationMessage message)
        {
            var viewModel = Window?.RootViewController?.DataContext() as IViewModel;
            if (viewModel != null && viewModel.Settings.State.Contains(IsRootConstant))
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Foreground, null, viewModel, this, message.Context));
        }

        public virtual IAsyncOperation TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (_hasRootPage)
                return null;

            if (WrapToNavigationController)
            {
                _hasRootPage = true;
                return parentPresenter.ShowAsync(context);
            }

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null || Window == null)
                return null;

            var mappingItem = ViewMappingProvider.FindMappingForViewModel(viewModel.GetType(), viewModel.GetViewName(context), false);
            if (mappingItem == null || !typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType))
                return null;

            if (!_subscribed)
            {
                _subscribed = true;
                EventAggregator.Subscribe(this);
            }
            _hasRootPage = true;
            viewModel.Settings.State.AddOrUpdate(IsRootConstant, null);
            var operation = new AsyncOperation<object>();
            OperationCallbackManager.Register(OperationType.PageNavigation, viewModel, operation.ToOperationCallback(), context);
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, context, (@this, vm, ctx) => @this.InitializeRootPage(vm, ctx));
            return operation;
        }

        public virtual Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null || Window == null)
                return null;
            if (viewModel.Settings.State.Contains(IsRootConstant))
            {
                var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModel, null, this, context);
                return NavigationDispatcher
                    .OnNavigatingAsync(navigationContext)
                    .TryExecuteSynchronously(task =>
                    {
                        if (task.Result)
                        {
                            var controller = Window.RootViewController;
                            _hasRootPage = false;
                            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, controller, navigationContext, (@this, c, ctx) =>
                            {
                                var w = @this.Window;
                                if (ReferenceEquals(w.RootViewController, c))
                                    w.RootViewController = null;
                                @this.NavigationDispatcher.OnNavigated(ctx);
                            });
                        }
                        return task.Result;
                    });
            }
            return null;
        }

        public virtual bool Restore(IDataContext context, IViewModelPresenter parentPresenter)
        {
            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (viewModel.Settings.State.Contains(IsRootConstant))
            {
                _hasRootPage = true;
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Refresh, null, viewModel, this, context));
                return true;
            }
            return false;
        }

        #endregion
    }
}