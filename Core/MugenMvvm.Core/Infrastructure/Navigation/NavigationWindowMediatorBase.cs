//using System;
//using System.ComponentModel;
//using System.Threading.Tasks;
//using MugenMvvm.Enums;
//using MugenMvvm.Interfaces.Metadata;
//using MugenMvvm.Interfaces.Navigation;
//using MugenMvvm.Interfaces.Threading;
//using MugenMvvm.Interfaces.ViewModels;
//using MugenMvvm.Interfaces.Views.Infrastructure;
//using MugenMvvm.Interfaces.Wrapping;
//
//namespace MugenMvvm.Infrastructure.Navigation
//{
//    public abstract class NavigationWindowMediatorBase<TView> : INavigationWindowMediator, IApplicationStateAwareNavigationProvider
//        where TView : class ?
//    {
//        #region Fields
//
//        private CancelEventArgs _cancelArgs;
//        private INavigationContext? _closingContext;
//        private INavigationContext? _showingContext;
//        private bool _shouldClose;
//
//        #endregion
//
//        #region Constructors
//
//        protected NavigationWindowMediatorBase(IViewManager viewManager, INavigationDispatcher navigationDispatcher, IThreadDispatcher threadDispatcher,
//            IWrapperManager wrapperManager)
//        {
//            Should.NotBeNull(viewManager, nameof(viewManager));
//            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
//            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
//            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
//            ViewManager = viewManager;
//            NavigationDispatcher = navigationDispatcher;
//            ThreadDispatcher = threadDispatcher;
//            WrapperManager = wrapperManager;
//        }
//
//        #endregion
//
//        #region Properties
//
//        protected IViewManager ViewManager { get; }
//
//        protected INavigationDispatcher NavigationDispatcher { get; }
//
//        protected IThreadDispatcher ThreadDispatcher { get; }
//
//        protected IWrapperManager WrapperManager { get; }
//
//        public virtual NavigationType NavigationType => NavigationType.Window;
//
//        public bool IsOpen { get; private set; }
//
//        object? INavigationWindowMediator.View => View;
//
//        public TView View { get; private set; }
//
//        public IViewModel ViewModel { get; private set; }
//
//        protected bool IsClosing => _closingContext != null;
//
//        #endregion
//
//        #region Implementation of interfaces
//
//        bool IApplicationStateAwareNavigationProvider.IsSupported(IViewModel viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
//        {
//            return ReferenceEquals(viewModel, ViewModel) && IsOpen && !viewModel.IsDisposed();
//        }
//
//        INavigationContext? IApplicationStateAwareNavigationProvider.TryCreateApplicationStateContext(IViewModel viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
//        {
//            return null;
//        }
//
//        public void Initialize(IViewModel viewModel, IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(viewModel, nameof(viewModel));
//            Should.NotBeNull(metadata, nameof(metadata));
//            if (ReferenceEquals(ViewModel, viewModel))
//                return;
//            if (ViewModel != null)
//                throw ExceptionManager.ObjectInitialized(GetType().Name, this);
//            ViewModel = viewModel;
//            OnInitialized(viewModel, metadata);
//        }
//
//        public IReadOnlyMetadataContext Show(IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(metadata, nameof(metadata));
//            ViewModel.NotBeDisposed();
//            return ShowInternal(true, metadata);
//        }
//
//        public IReadOnlyMetadataContext Close(IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(metadata, nameof(metadata));
//            return CloseInternal(true, metadata);
//        }
//
//        public IReadOnlyMetadataContext Restore(object view, IReadOnlyMetadataContext metadata)
//        {
//            Should.NotBeNull(metadata, nameof(metadata));
//            return RestoreInternal(view, metadata);
//        }
//
//        #endregion
//
//        #region Methods
//
//        protected abstract void ShowView(IReadOnlyMetadataContext metadata);
//
//        protected abstract bool ActivateView(IReadOnlyMetadataContext metadata);
//
//        protected abstract void InitializeView(IReadOnlyMetadataContext metadata);
//
//        protected abstract void CloseView(IReadOnlyMetadataContext metadata);
//
//        protected abstract void CleanupView(TView view, IReadOnlyMetadataContext metadata);
//
//        protected virtual void OnInitialized(IViewModel viewModel, IReadOnlyMetadataContext metadata)
//        {
//        }
//
//        protected virtual IReadOnlyMetadataContext ShowInternal(bool shouldWaitNavigation, IReadOnlyMetadataContext metadata)
//        {
//            if (shouldWaitNavigation)
//            {
//                NavigationDispatcher
//                    .WaitNavigationAsync(ShouldWaitNavigationBeforeShow, metadata)
//                    .ContinueWith(ShowAfterWaitNavigation, metadata, TaskContinuationOptions.ExecuteSynchronously);
//                return metadata;
//            }
//
//            if (IsOpen)
//            {
//                ThreadDispatcher.Execute(RefreshCallback, ThreadExecutionMode.Main, metadata);
//                return metadata;
//            }
//
//            NavigationDispatcher
//                .OnNavigatingTo(this, NavigationType, ViewModel, metadata: metadata)
//                .CompleteNavigation((dispatcher, context) =>
//                {
//                    IsOpen = true;
//                    ViewManager
//                        .GetViewAsync(ViewModel, context.Metadata)
//                        .ContinueWith(OnViewCreated, context, TaskContinuationOptions.ExecuteSynchronously);
//                    return false;
//                });
//            return metadata;
//        }
//
//        protected virtual IReadOnlyMetadataContext CloseInternal(bool shouldWaitNavigation, IReadOnlyMetadataContext metadata)
//        {
//            if (!IsOpen)
//            {
//                Tracer.Error(MessageConstants.CannotCloseMediator);
//                return metadata;
//            }
//
//            if (IsClosing)
//                return metadata;
//
//            if (shouldWaitNavigation)
//            {
//                NavigationDispatcher
//                    .WaitNavigationAsync(ShouldWaitNavigationBeforeClose, metadata)
//                    .ContinueWith(CloseAfterWaitNavigation, metadata, TaskContinuationOptions.ExecuteSynchronously);
//                return metadata;
//            }
//
//            _closingContext = NavigationDispatcher.CreateNavigateFromContext(this, NavigationType, ViewModel, metadata: metadata);
//            NavigationDispatcher.OnNavigating(_closingContext).CompleteNavigation((dispatcher, context) =>
//            {
//                ThreadDispatcher.Execute(CloseViewCallback, ThreadExecutionMode.Main, context);
//                return false;
//            }, (dispatcher, context, arg3) => _closingContext = null);
//            return metadata;
//        }
//
//        protected virtual IReadOnlyMetadataContext RestoreInternal(object view, IReadOnlyMetadataContext metadata)
//        {
//            UpdateView(view, true, metadata);
//            NavigationDispatcher.OnNavigated(NavigationDispatcher.CreateNavigateToContext(this, NavigationType, ViewModel, NavigationMode.Restore));
//            return metadata;
//        }
//
//        protected virtual bool ShouldWaitNavigationBeforeShow(INavigationCallback callback)
//        {
//            return callback.NavigationType == NavigationType && callback.CallbackType == NavigationCallbackType.Showing;
//        }
//
//        protected virtual bool ShouldWaitNavigationBeforeClose(INavigationCallback callback)
//        {
//            return false;
//        }
//
//        protected void UpdateView(object? view, bool isOpen, IReadOnlyMetadataContext metadata)
//        {
//            var oldView = View;
//            if (view != null)
//            {
//                view = WrapperManager.Wrap(view, typeof(TView), metadata);
//                if (ReferenceEquals(oldView, view))
//                    return;
//            }
//
//            IsOpen = isOpen;
//            if (oldView != null)
//                CleanupView(oldView, metadata);
//            View = (TView)view;
//            if (View != null)
//                InitializeView(metadata);
//        }
//
//        protected void OnViewOpened(object? sender = null, EventArgs? e = null)
//        {
//            var navigationContext = _showingContext;
//            _showingContext = null;
//            if (navigationContext == null)
//                navigationContext = NavigationDispatcher.CreateNavigateToContext(this, NavigationType, ViewModel);
//            NavigationDispatcher.OnNavigated(navigationContext);
//        }
//
//        protected void OnViewActivated(object? sender = null, EventArgs? e = null)
//        {
//            var navigationContext = _showingContext;
//            _showingContext = null;
//            if (navigationContext == null)
//                navigationContext = NavigationDispatcher.CreateNavigateToContext(this, NavigationType, ViewModel, NavigationMode.Refresh);
//            NavigationDispatcher.OnNavigated(navigationContext);
//        }
//
//        protected void OnViewClosing(object sender, CancelEventArgs e)
//        {
//            try
//            {
//                _cancelArgs = e;
//                if (_shouldClose)
//                {
//                    e.Cancel = false;
//                    CompleteClose(_closingContext);
//                    return;
//                }
//
//                e.Cancel = true;
//                CloseInternal(false, Default.MetadataContext);
//            }
//            finally
//            {
//                _cancelArgs = null;
//            }
//        }
//
//        protected void OnViewClosed(object? sender = null, EventArgs? e = null)
//        {
//            if (!IsClosing)
//                CompleteClose(_closingContext);
//        }
//
//        private void OnViewCreated(Task<object> task, object state)
//        {
//            var context = (INavigationContext)state;
//            View = (TView)WrapperManager.Wrap(task.Result, typeof(TView), context.Metadata);
//            ViewManager.InitializeViewAsync(task.Result, ViewModel, context.Metadata);
//            InitializeView(context.Metadata);
//
//            ThreadDispatcher.Execute(o =>
//            {
//                try
//                {
//                    _showingContext = (INavigationContext)o;
//                    ShowView(_showingContext.Metadata);
//                }
//                catch (Exception e)
//                {
//                    _showingContext = null;
//                    NavigationDispatcher.OnNavigationFailed((INavigationContext)o, e);
//                    throw;
//                }
//            }, ThreadExecutionMode.Main, context);
//        }
//
//        private void ShowAfterWaitNavigation(Task task, object state)
//        {
//            ViewModel.NotBeDisposed();
//            ShowInternal(false, (IReadOnlyMetadataContext)state);
//        }
//
//        private void CloseAfterWaitNavigation(Task task, object state)
//        {
//            ViewModel.NotBeDisposed();
//            CloseInternal(false, (IReadOnlyMetadataContext)state);
//        }
//
//        private void CompleteClose(INavigationContext? context)
//        {
//            if (context == null)
//                context = NavigationDispatcher.CreateNavigateFromContext(this, NavigationType, ViewModel);
//            NavigationDispatcher.OnNavigated(context);
//            _closingContext = null;
//            _shouldClose = false;
//            IsOpen = false;
//            var view = View;
//            if (view != null)
//            {
//                CleanupView(view, context.Metadata);
//                ViewManager.CleanupViewAsync(ViewModel, context.Metadata);
//                View = null;
//            }
//        }
//
//        private void RefreshCallback(object state)
//        {
//            var ctx = NavigationDispatcher.CreateNavigateToContext(this, NavigationType, ViewModel, NavigationMode.Refresh, (IReadOnlyMetadataContext)state);
//            try
//            {
//                _showingContext = ctx;
//                if (!ActivateView(ctx.Metadata))
//                {
//                    _showingContext = null;
//                    NavigationDispatcher.OnNavigationCanceled(ctx);
//                }
//            }
//            catch (Exception e)
//            {
//                _showingContext = null;
//                NavigationDispatcher.OnNavigationFailed(ctx, e);
//                throw;
//            }
//        }
//
//        private void CloseViewCallback(object state)
//        {
//            var navigationContext = (INavigationContext)state;
//            try
//            {
//                if (_cancelArgs != null)
//                {
//                    _cancelArgs.Cancel = false;
//                    CompleteClose(navigationContext);
//                }
//                else if (View != null)
//                {
//                    _shouldClose = true;
//                    CloseView(navigationContext.Metadata);
//                }
//            }
//            catch (Exception e)
//            {
//                NavigationDispatcher.OnNavigationFailed(navigationContext, e);
//                throw;
//            }
//        }
//
//        #endregion
//    }
//}