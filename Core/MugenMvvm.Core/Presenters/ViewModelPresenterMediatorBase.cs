using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;

namespace MugenMvvm.Presenters
{
    public abstract class ViewModelPresenterMediatorBase<TView> : IViewModelPresenterMediator, INavigationProvider
            where TView : class
    {
        #region Fields

        private CancelEventArgs? _cancelArgs;
        private INavigationContext? _closingContext;
        private bool _shouldClose;
        private INavigationContext? _showingContext;
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        protected ViewModelPresenterMediatorBase(INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
            _threadDispatcher = threadDispatcher;
        }
#pragma warning restore CS8618

        #endregion

        #region Properties

        public virtual string Id => GetType().FullName;

        public abstract NavigationType NavigationType { get; }

        public bool IsOpen { get; private set; }

        public IViewInfo? ViewInfo { get; private set; }

        public IViewModelBase ViewModel { get; private set; }

        public IViewInitializer ViewInitializer { get; private set; }

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected TView? View { get; private set; }

        protected bool IsClosing => _closingContext != null;

        protected virtual ThreadExecutionMode ExecutionMode => ThreadExecutionMode.Main;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IViewModelBase viewModel, IViewInitializer viewInitializer, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(viewInitializer, nameof(viewInitializer));
            if (ReferenceEquals(ViewModel, viewModel))
                return;
            if (ViewModel != null)
                ExceptionManager.ThrowObjectInitialized(this);

            ViewModel = viewModel;
            ViewInitializer = viewInitializer;
            OnInitialized(metadata);
        }

        public IPresenterResult Show(IReadOnlyMetadataContext? metadata = null)
        {
            ViewModel.NotBeDisposed();
            return ShowInternal(true, metadata);
        }

        public IPresenterResult Close(IReadOnlyMetadataContext? metadata = null)
        {
            return CloseInternal(true, metadata);
        }

        public IPresenterResult Restore(IViewInfo viewInfo, IReadOnlyMetadataContext? metadata = null)
        {
            return RestoreInternal(viewInfo, metadata);
        }

        #endregion

        #region Methods

        protected abstract void ShowView(IReadOnlyMetadataContext? metadata);

        protected abstract bool ActivateView(IReadOnlyMetadataContext? metadata);

        protected abstract void InitializeView(IReadOnlyMetadataContext? metadata);

        protected abstract void CloseView(IReadOnlyMetadataContext? metadata);

        protected abstract void CleanupView(TView view, IReadOnlyMetadataContext? metadata);

        protected virtual void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual IPresenterResult ShowInternal(bool shouldWaitNavigation, IReadOnlyMetadataContext? metadata)
        {
            if (shouldWaitNavigation)
            {
                NavigationDispatcher
                    .WaitNavigationAsync(ShouldWaitNavigationBeforeShow, metadata)
                    .ContinueWith(ShowAfterWaitNavigation, metadata, TaskContinuationOptions.ExecuteSynchronously);
                return this.GetPresenterResult(NavigationType, ViewModel);
            }

            if (IsOpen)
            {
                ThreadDispatcher.Execute(ExecutionMode, RefreshCallback, metadata);
                return this.GetPresenterResult(NavigationType, ViewModel);
            }

            NavigationDispatcher
                .OnNavigating(NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.New, metadata), (dispatcher, context) =>
                {
                    IsOpen = true;
                    ViewInitializer
                        .InitializeAsync(ViewModel, context.Metadata)
                        .ContinueWith(OnViewInitialized, context, TaskContinuationOptions.ExecuteSynchronously);
                    return false;
                });
            return this.GetPresenterResult(NavigationType, ViewModel);
        }

        protected virtual IPresenterResult CloseInternal(bool shouldWaitNavigation, IReadOnlyMetadataContext? metadata)
        {
            if (!IsOpen)
            {
                Tracer.Error()?.Trace(MessageConstant.CannotCloseMediator);
                return this.GetPresenterResult(NavigationType, ViewModel);
            }

            if (IsClosing)
                return this.GetPresenterResult(NavigationType, ViewModel);

            if (shouldWaitNavigation)
            {
                NavigationDispatcher
                    .WaitNavigationAsync(ShouldWaitNavigationBeforeClose, metadata)
                    .ContinueWith(CloseAfterWaitNavigation, metadata, TaskContinuationOptions.ExecuteSynchronously);
                return this.GetPresenterResult(NavigationType, ViewModel);
            }

            _closingContext = NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.Back, metadata);
            NavigationDispatcher.OnNavigating(_closingContext, (dispatcher, context) =>
            {
                ThreadDispatcher.Execute(ExecutionMode, CloseViewCallback, context);
                return false;
            }, (dispatcher, context, arg3) => _closingContext = null);
            return this.GetPresenterResult(NavigationType, ViewModel);
        }

        protected virtual IPresenterResult RestoreInternal(IViewInfo viewInfo, IReadOnlyMetadataContext? metadata)
        {
            UpdateView(viewInfo, true, metadata);
            NavigationDispatcher.OnNavigated(NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.Restore, metadata));
            return this.GetPresenterResult(NavigationType, ViewModel);
        }

        protected virtual bool ShouldWaitNavigationBeforeShow(INavigationCallback callback)
        {
            return callback.NavigationType == NavigationType && callback.CallbackType == NavigationCallbackType.Showing;
        }

        protected virtual bool ShouldWaitNavigationBeforeClose(INavigationCallback callback)
        {
            return false;
        }

        protected void UpdateView(IViewInfo? viewInfo, bool isOpen, IReadOnlyMetadataContext? metadata)
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
                navigationContext = NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.New);
            NavigationDispatcher.OnNavigated(navigationContext);
        }

        protected void OnViewActivated(object? sender = null, EventArgs? e = null)
        {
            var navigationContext = _showingContext;
            _showingContext = null;
            if (navigationContext == null)
                navigationContext = NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.Refresh);
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
                CloseInternal(false, null);
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

        private void OnViewInitialized(Task<IViewInitializerResult> task, object state)
        {
            var navigationContext = (INavigationContext)state;
            UpdateView(task.Result.ViewInfo, true, navigationContext.Metadata);

            ThreadDispatcher.Execute(ExecutionMode, ViewInitializedCallback, navigationContext);
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
                navigationContext = NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.Back);
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

        private void ViewInitializedCallback(INavigationContext state)
        {
            try
            {
                _showingContext = state;
                ShowView(_showingContext.Metadata);
            }
            catch (Exception e)
            {
                _showingContext = null;
                NavigationDispatcher.OnNavigationFailed(state, e);
                throw;
            }
        }

        private void RefreshCallback(IReadOnlyMetadataContext? state)
        {
            var ctx = NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationType, NavigationMode.Refresh, state);
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

        private void CloseViewCallback(INavigationContext navigationContext)
        {
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