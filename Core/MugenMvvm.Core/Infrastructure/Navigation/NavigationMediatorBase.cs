using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Navigation
{
    public abstract class NavigationMediatorBase<TView> : INavigationMediator, IApplicationStateAwareNavigationProvider
        where TView : class ?
    {
        #region Fields

        private CancelEventArgs _cancelArgs;
        private INavigationContext? _closingContext;
        private bool _shouldClose;
        private INavigationContext? _showingContext;

        #endregion

        #region Constructors

        protected NavigationMediatorBase(INavigationDispatcher navigationDispatcher, IThreadDispatcher threadDispatcher)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            NavigationDispatcher = navigationDispatcher;
            ThreadDispatcher = threadDispatcher;
            Id = GetType().Name + Default.NextCounter();
        }

        #endregion

        #region Properties

        public virtual NavigationType NavigationType => NavigationType.Generic;

        public bool IsOpen { get; private set; }

        public IViewInfo? ViewInfo { get; private set; }

        public IViewModelBase ViewModel { get; private set; }

        public IViewInitializer ViewInitializer { get; private set; }

        public string Id { get; }

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected TView View { get; private set; }

        protected bool IsClosing => _closingContext != null;

        #endregion

        #region Implementation of interfaces

        bool IApplicationStateAwareNavigationProvider.IsSupported(IViewModelBase viewModel, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
        {
            return ReferenceEquals(viewModel, ViewModel) && IsOpen && !viewModel.IsDisposed();
        }

        INavigationContext? IApplicationStateAwareNavigationProvider.TryCreateApplicationStateContext(IViewModelBase viewModel, ApplicationState oldState,
            ApplicationState newState, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        public void Initialize(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(viewInitializer, nameof(viewInitializer));
            Should.NotBeNull(metadata, nameof(metadata));
            if (ReferenceEquals(ViewModel, viewModel))
                return;
            if (ViewModel != null)
                throw ExceptionManager.ObjectInitialized(GetType().Name, this);
            ViewModel = viewModel;
            ViewInitializer = viewInitializer;
            OnInitialized(metadata);
        }

        public IReadOnlyMetadataContext Show(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            ViewModel.NotBeDisposed();
            return ShowInternal(true, metadata);
        }

        public IReadOnlyMetadataContext Close(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return CloseInternal(true, metadata);
        }

        public IReadOnlyMetadataContext Restore(IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return RestoreInternal(viewInfo, metadata);
        }

        #endregion

        #region Methods

        protected abstract void ShowView(IReadOnlyMetadataContext metadata);

        protected abstract bool ActivateView(IReadOnlyMetadataContext metadata);

        protected abstract void InitializeView(IReadOnlyMetadataContext metadata);

        protected abstract void CloseView(IReadOnlyMetadataContext metadata);

        protected abstract void CleanupView(TView view, IReadOnlyMetadataContext metadata);

        protected virtual void OnInitialized(IReadOnlyMetadataContext metadata)
        {
        }

        protected virtual IReadOnlyMetadataContext ShowInternal(bool shouldWaitNavigation, IReadOnlyMetadataContext metadata)
        {
            if (shouldWaitNavigation)
            {
                NavigationDispatcher
                    .WaitNavigationAsync(ShouldWaitNavigationBeforeShow, metadata)
                    .ContinueWith(ShowAfterWaitNavigation, metadata, TaskContinuationOptions.ExecuteSynchronously);
                return Default.MetadataContext;
            }

            if (IsOpen)
            {
                ThreadDispatcher.Execute(RefreshCallback, ThreadExecutionMode.Main, metadata);
                return Default.MetadataContext;
            }

            NavigationDispatcher
                .OnNavigatingTo(this, NavigationMode.New, NavigationType, ViewModel, metadata)
                .CompleteNavigation((dispatcher, context) =>
                {
                    IsOpen = true;
                    ViewInitializer
                        .InitializeAsync(ViewModel, context.Metadata)
                        .ContinueWith(OnViewInitialized, context, TaskContinuationOptions.ExecuteSynchronously);
                    return false;
                });
            return Default.MetadataContext;
        }

        protected virtual IReadOnlyMetadataContext CloseInternal(bool shouldWaitNavigation, IReadOnlyMetadataContext metadata)
        {
            if (!IsOpen)
            {
                Tracer.Error(MessageConstants.CannotCloseMediator);
                return Default.MetadataContext;
            }

            if (IsClosing)
                return Default.MetadataContext;

            if (shouldWaitNavigation)
            {
                NavigationDispatcher
                    .WaitNavigationAsync(ShouldWaitNavigationBeforeClose, metadata)
                    .ContinueWith(CloseAfterWaitNavigation, metadata, TaskContinuationOptions.ExecuteSynchronously);
                return Default.MetadataContext;
            }

            _closingContext = NavigationDispatcher.ContextFactory.GetNavigationContextFrom(this, NavigationMode.Back, NavigationType, ViewModel, metadata);
            NavigationDispatcher.OnNavigating(_closingContext).CompleteNavigation((dispatcher, context) =>
            {
                ThreadDispatcher.Execute(CloseViewCallback, ThreadExecutionMode.Main, context);
                return false;
            }, (dispatcher, context, arg3) => _closingContext = null);
            return Default.MetadataContext;
        }

        protected virtual IReadOnlyMetadataContext RestoreInternal(IViewInfo viewInfo, IReadOnlyMetadataContext metadata)
        {
            UpdateView(viewInfo, true, metadata);
            NavigationDispatcher.OnNavigated(NavigationDispatcher.ContextFactory.GetNavigationContextTo(this, NavigationMode.Restore, NavigationType, ViewModel, metadata));
            return Default.MetadataContext;
        }

        protected virtual bool ShouldWaitNavigationBeforeShow(INavigationCallback callback)
        {
            return callback.NavigationType == NavigationType && callback.CallbackType == NavigationCallbackType.Showing;
        }

        protected virtual bool ShouldWaitNavigationBeforeClose(INavigationCallback callback)
        {
            return false;
        }

        protected void UpdateView(IViewInfo? viewInfo, bool isOpen, IReadOnlyMetadataContext metadata)
        {
            if (ReferenceEquals(viewInfo, ViewInfo))
                return;

            ViewInfo = viewInfo;
            var oldView = View;
            View = viewInfo?.Wrap<TView>(metadata);
            if (ReferenceEquals(oldView, View))
                return;

            IsOpen = isOpen;
            if (oldView != null)
                CleanupView(oldView, metadata);
            if (View != null)
                InitializeView(metadata);
        }

        protected void OnViewShown(object? sender = null, EventArgs? e = null)
        {
            var navigationContext = _showingContext;
            _showingContext = null;
            if (navigationContext == null)
                navigationContext = NavigationDispatcher.ContextFactory.GetNavigationContextTo(this, NavigationMode.New, NavigationType, ViewModel, Default.MetadataContext);
            NavigationDispatcher.OnNavigated(navigationContext);
        }

        protected void OnViewActivated(object? sender = null, EventArgs? e = null)
        {
            var navigationContext = _showingContext;
            _showingContext = null;
            if (navigationContext == null)
                navigationContext = NavigationDispatcher.ContextFactory.GetNavigationContextTo(this, NavigationMode.Refresh, NavigationType, ViewModel, Default.MetadataContext);
            NavigationDispatcher.OnNavigated(navigationContext);
        }

        protected void OnViewClosing(object sender, CancelEventArgs e)
        {
            try
            {
                _cancelArgs = e;
                if (_shouldClose)
                {
                    e.Cancel = false;
                    CompleteClose(_closingContext);
                    return;
                }

                e.Cancel = true;
                CloseInternal(false, Default.MetadataContext);
            }
            finally
            {
                _cancelArgs = null;
            }
        }

        protected void OnViewClosed(object? sender = null, EventArgs? e = null)
        {
            if (!IsClosing)
                CompleteClose(_closingContext);
        }

        private void OnViewInitialized(Task<IViewManagerResult> task, object state)
        {
            var navigationContext = (INavigationContext)state;
            UpdateView(task.Result.ViewInfo, true, navigationContext.Metadata);

            ThreadDispatcher.Execute(o =>
            {
                try
                {
                    _showingContext = (INavigationContext)o;
                    ShowView(_showingContext.Metadata);
                }
                catch (Exception e)
                {
                    _showingContext = null;
                    NavigationDispatcher.OnNavigationFailed((INavigationContext)o, e);
                    throw;
                }
            }, ThreadExecutionMode.Main, navigationContext);
        }

        private void ShowAfterWaitNavigation(Task task, object state)
        {
            ViewModel.NotBeDisposed();
            ShowInternal(false, (IReadOnlyMetadataContext)state);
        }

        private void CloseAfterWaitNavigation(Task task, object state)
        {
            ViewModel.NotBeDisposed();
            CloseInternal(false, (IReadOnlyMetadataContext)state);
        }

        private void CompleteClose(INavigationContext? navigationContext)
        {
            if (navigationContext == null)
                navigationContext = NavigationDispatcher.ContextFactory.GetNavigationContextFrom(this, NavigationMode.Back, NavigationType, ViewModel, Default.MetadataContext);
            NavigationDispatcher.OnNavigated(navigationContext);
            _closingContext = null;
            _shouldClose = false;
            IsOpen = false;
            var view = View;
            if (view != null)
            {
                CleanupView(view, navigationContext.Metadata);
                ViewInfo?.CleanupAsync(ViewModel, navigationContext.Metadata);
                ViewInfo = null;
                View = null;
            }
        }

        private void RefreshCallback(object state)
        {
            var ctx = NavigationDispatcher.ContextFactory.GetNavigationContextTo(this, NavigationMode.Refresh, NavigationType, ViewModel, (IReadOnlyMetadataContext) state);
            try
            {
                _showingContext = ctx;
                if (!ActivateView(ctx.Metadata))
                {
                    _showingContext = null;
                    NavigationDispatcher.OnNavigationCanceled(ctx);
                }
            }
            catch (Exception e)
            {
                _showingContext = null;
                NavigationDispatcher.OnNavigationFailed(ctx, e);
                throw;
            }
        }

        private void CloseViewCallback(object state)
        {
            var navigationContext = (INavigationContext)state;
            try
            {
                if (_cancelArgs != null)
                {
                    _cancelArgs.Cancel = false;
                    CompleteClose(navigationContext);
                }
                else if (View != null)
                {
                    _shouldClose = true;
                    CloseView(navigationContext.Metadata);
                }
            }
            catch (Exception e)
            {
                NavigationDispatcher.OnNavigationFailed(navigationContext, e);
                throw;
            }
        }

        #endregion
    }
}