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
    public class TouchRootDynamicViewModelPresenter : IRestorableDynamicViewModelPresenter, IHandler<BackgroundNavigationMessage>, IHandler<ForegroundNavigationMessage>
    {
        #region Fields

        protected static readonly DataConstant<object> IsRootConstant;
        private bool _initialized;

        #endregion

        #region Constructors

        static TouchRootDynamicViewModelPresenter()
        {
            IsRootConstant = DataConstant.Create<object>(typeof(TouchRootDynamicViewModelPresenter), nameof(IsRootConstant), false);
        }

        [Preserve(Conditional = true)]
        public TouchRootDynamicViewModelPresenter(IViewManager viewManager, INavigationDispatcher navigationDispatcher,
            IViewMappingProvider viewMappingProvider, IThreadManager threadManager, IEventAggregator eventAggregator)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(viewMappingProvider, nameof(viewMappingProvider));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(eventAggregator, nameof(eventAggregator));
            ViewManager = viewManager;
            NavigationDispatcher = navigationDispatcher;
            ViewMappingProvider = viewMappingProvider;
            ThreadManager = threadManager;
            eventAggregator.Subscribe(this);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected IThreadManager ThreadManager { get; }

        protected IViewManager ViewManager { get; }

        protected IViewMappingProvider ViewMappingProvider { get; }

        public UIWindow Window { get; set; }

        int IDynamicViewModelPresenter.Priority => int.MaxValue - 1;

        #endregion

        #region Methods

        protected virtual void InitializeRootView(IViewModel viewModel, IDataContext context)
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
            if (_initialized)
                return null;

            if (context.GetData(NavigationConstants.SuppressRootNavigation))
                return null;

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null || Window == null)
                return null;

            var mappingItem = ViewMappingProvider.FindMappingForViewModel(viewModel.GetType(), viewModel.GetViewName(context), false);
            if (mappingItem == null || !typeof(UIViewController).IsAssignableFrom(mappingItem.ViewType))
                return null;

            _initialized = true;

            viewModel.Settings.State.AddOrUpdate(IsRootConstant, null);
            viewModel.Settings.Metadata.Add(ViewModelConstants.CanCloseHandler, (model, o) => false);
            var operation = viewModel.RegisterNavigationOperation(OperationType.PageNavigation, context);
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, context, (@this, vm, ctx) => @this.InitializeRootView(vm, ctx));
            return operation;
        }

        public virtual Task<bool> TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        public virtual bool Restore(IDataContext context, IViewModelPresenter parentPresenter)
        {
            _initialized = true;

            var viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;

            if (viewModel.Settings.State.Contains(IsRootConstant))
            {
                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.CanCloseHandler, (model, o) => false);
                NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Refresh, null, viewModel, this, context));
                return true;
            }
            return false;
        }

        #endregion
    }
}