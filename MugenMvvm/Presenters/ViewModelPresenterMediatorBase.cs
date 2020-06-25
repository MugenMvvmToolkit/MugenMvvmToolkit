using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Presenters
{
    public abstract class ViewModelPresenterMediatorBase<TView> : IViewModelPresenterMediator, INavigationProvider
        where TView : class
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly IViewManager? _viewManager;
        private readonly IWrapperManager? _wrapperManager;

        private CancelEventArgs? _cancelArgs;
        private INavigationContext? _closingContext;

        private string? _id;
        private IViewModelViewMapping? _mapping;
        private string? _navigationId;
        private bool _shouldClose;
        private INavigationContext? _showingContext;
        private IViewModelBase? _viewModel;

        #endregion

        #region Constructors

        protected ViewModelPresenterMediatorBase(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
        {
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _navigationDispatcher = navigationDispatcher;
            _threadDispatcher = threadDispatcher;
        }

        #endregion

        #region Properties

        public string Id
        {
            get
            {
                if (_id == null)
                    ExceptionManager.ThrowObjectNotInitialized(this);
                return _id;
            }
            protected set
            {
                Should.NotBeNullOrEmpty(value, nameof(value));
                _id = value;
            }
        }

        public abstract NavigationType NavigationType { get; }

        protected IView? View { get; private set; }

        protected IViewModelBase ViewModel
        {
            get
            {
                if (_viewModel == null)
                    ExceptionManager.ThrowObjectNotInitialized(this);
                return _viewModel;
            }
            private set => _viewModel = value;
        }

        protected IViewModelViewMapping Mapping
        {
            get
            {
                if (_mapping == null)
                    ExceptionManager.ThrowObjectNotInitialized(this);
                return _mapping;
            }
            private set => _mapping = value;
        }

        protected TView? CurrentView { get; private set; }

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected IWrapperManager WrapperManager => _wrapperManager.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected virtual ThreadExecutionMode ExecutionMode => ThreadExecutionMode.Main;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IViewModelBase viewModel, IViewModelViewMapping mapping, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            if (ReferenceEquals(_viewModel, viewModel) && ReferenceEquals(_mapping, mapping))
                return;
            if (_viewModel != null)
                ExceptionManager.ThrowObjectInitialized(this);

            ViewModel = viewModel;
            Mapping = mapping;
            if (string.IsNullOrEmpty(_id))
                _id = $"{GetType().FullName}{mapping.Id}";
            _navigationId = this.GetNavigationId(viewModel);
            OnInitialized(metadata);
        }

        public virtual IPresenterResult? TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_showingContext != null)
                return GetPresenterResult(false, _showingContext?.Metadata ?? metadata);

            WaitNavigationBeforeShowAsync(view, cancellationToken, metadata).ContinueWith((task, s) =>
            {
                var tuple = (Tuple<ViewModelPresenterMediatorBase<TView>, object?, CancellationToken, IReadOnlyMetadataContext?>)s;
                tuple.Item1.ShowInternal(tuple.Item2, tuple.Item3, tuple.Item4);
            }, Tuple.Create(this, view, cancellationToken, metadata), TaskContinuationOptions.ExecuteSynchronously);

            return GetPresenterResult(true, metadata);
        }

        public virtual IPresenterResult? TryClose(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (View == null)
                return null;

            if (_closingContext != null)
                return GetPresenterResult(false, _closingContext?.Metadata ?? metadata);

            WaitNavigationBeforeCloseAsync(cancellationToken, metadata).ContinueWith((task, s) =>
            {
                var tuple = (Tuple<ViewModelPresenterMediatorBase<TView>, CancellationToken, IReadOnlyMetadataContext?>)s;
                tuple.Item1.CloseInternal(tuple.Item2, tuple.Item3);
            }, Tuple.Create(this, cancellationToken, metadata), TaskContinuationOptions.ExecuteSynchronously);
            return GetPresenterResult(false, metadata);
        }

        #endregion

        #region Methods

        protected abstract void ShowView(INavigationContext context);

        protected abstract void InitializeView(INavigationContext context);

        protected abstract void CloseView(INavigationContext context);

        protected abstract void CleanupView(INavigationContext context);

        protected virtual bool ActivateView(INavigationContext context)
        {
            OnViewActivated();
            return true;
        }

        protected virtual void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual Task WaitNavigationBeforeShowAsync(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == state.NavigationType && callback.CallbackType == NavigationCallbackType.Showing, metadata);
        }

        protected virtual Task WaitNavigationBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return Task.CompletedTask;
        }

        protected virtual IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata)
        {
            return new PresenterResult(ViewModel, _navigationId!, this, NavigationType, metadata);
        }

        protected virtual INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata)
        {
            return NavigationDispatcher.GetNavigationContext(ViewModel, this, _navigationId!, NavigationType, mode, metadata);
        }

        protected virtual NavigationMode GetShowNavigationMode(object? view, IReadOnlyMetadataContext? metadata)
        {
            if (View != null)
                return NavigationMode.Refresh;

            var hashSet = ViewModel.GetMetadataOrDefault().Get(NavigationMetadata.OpenedNavigationProviders);
            if (hashSet != null)
            {
                lock (hashSet)
                {
                    if (hashSet.Contains(Id))
                        return NavigationMode.Restore;
                }
            }

            return NavigationMode.New;
        }

        protected virtual void OnNavigated(INavigationContext navigationContext)
        {
            var hashSet = ViewModel.Metadata.GetOrAdd(NavigationMetadata.OpenedNavigationProviders, this, (_, __) => new HashSet<string>(StringComparer.Ordinal));
            lock (hashSet)
            {
                if (navigationContext.NavigationMode.IsClose)
                    hashSet.Remove(Id);
                else
                    hashSet.Add(Id);
            }

            ClearFields(navigationContext.NavigationMode.IsClose);
            NavigationDispatcher.OnNavigated(navigationContext);

            if (!navigationContext.NavigationMode.IsClose)
                return;

            var view = View;
            if (view == null)
                return;

            if (CurrentView != null)
                CleanupView(navigationContext);
            ViewManager.CleanupAsync(view, navigationContext, default, navigationContext.GetMetadataOrDefault());
            CurrentView = null;
            View = null;
        }

        protected virtual void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            ClearFields(navigationContext.NavigationMode.IsClose);
            NavigationDispatcher.OnNavigationFailed(navigationContext, exception);
        }

        protected virtual void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            ClearFields(navigationContext.NavigationMode.IsClose);
            NavigationDispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
        }

        protected internal void OnViewShown(object? sender = null, EventArgs? e = null)
        {
            OnNavigated(_showingContext ?? GetNavigationContext(NavigationMode.New, null));
        }

        protected internal void OnViewActivated(object? sender = null, EventArgs? e = null)
        {
            OnNavigated(_showingContext ?? GetNavigationContext(NavigationMode.Refresh, null));
        }

        protected internal void OnViewClosing(object sender, CancelEventArgs e)
        {
            try
            {
                _cancelArgs = e;
                if (_shouldClose)
                    e.Cancel = false;
                else
                {
                    e.Cancel = true;
                    CloseInternal(default, null);
                }
            }
            finally
            {
                _cancelArgs = null;
            }
        }

        protected internal void OnViewClosed(object? sender = null, EventArgs? e = null)
        {
            OnNavigated(_closingContext ?? GetNavigationContext(NavigationMode.Close, null));
        }

        protected void ShowInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var navigationContext = GetNavigationContext(GetShowNavigationMode(view, metadata), metadata);
            if (ViewModel.IsDisposed())
            {
                OnNavigationFailed(navigationContext, new ObjectDisposedException(ViewModel.GetType().FullName));
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                OnNavigationCanceled(navigationContext, cancellationToken);
                return;
            }

            _showingContext = navigationContext;
            if (navigationContext.NavigationMode.IsNew)
            {
                NavigationDispatcher.OnNavigating(navigationContext, (this, view, cancellationToken), (dispatcher, context, state) =>
                {
                    state.Item1.ViewManager
                        .InitializeAsync(state.Item1.Mapping, new ViewModelViewRequest(state.Item1.ViewModel, state.Item2), state.Item3, context.Metadata)
                        .ContinueWith(state.Item1.OnViewInitializedShowCallback, context, TaskContinuationOptions.ExecuteSynchronously);
                    return false;
                }, (dispatcher, context, ex, state) => state.Item1._showingContext = null, cancellationToken);
            }
            else
            {
                if (CurrentView != null && (view == null || ReferenceEquals(CurrentView, view)))
                    ThreadDispatcher.Execute(ExecutionMode, navigationContext, RefreshCallback, metadata);
                else
                {
                    ViewManager
                        .InitializeAsync(Mapping, new ViewModelViewRequest(ViewModel, view), cancellationToken, metadata)
                        .ContinueWith(view == null ? OnViewInitializedShowCallback : (Action<Task<IView>, object>)OnViewInitializedRefreshCallback, navigationContext, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
        }

        protected void CloseInternal(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _closingContext = GetNavigationContext(NavigationMode.Close, metadata);
            NavigationDispatcher.OnNavigating(_closingContext, this, (dispatcher, context, state) =>
            {
                state.ThreadDispatcher.Execute(state.ExecutionMode, context, state.CloseViewCallback);
                return false;
            }, (dispatcher, context, ex, state) => state._closingContext = null, cancellationToken);
        }

        private void UpdateView(IView? view, INavigationContext context)
        {
            if (ReferenceEquals(view, View))
                return;

            View = view;
            var oldView = CurrentView;
            var newView = view?.Wrap<TView>(context.GetMetadataOrDefault(), _wrapperManager);
            if (ReferenceEquals(oldView, newView))
                return;

            if (CurrentView != null)
                CleanupView(context);
            CurrentView = newView;
            if (CurrentView != null)
                InitializeView(context);
        }

        private void OnViewInitializedShowCallback(Task<IView> task, object state)
        {
            ThreadDispatcher.Execute(ExecutionMode, (this, task, (INavigationContext)state), s => s.Item1.ShowViewCallback(s.task, s.Item3, true));
        }

        private void OnViewInitializedRefreshCallback(Task<IView> task, object state)
        {
            ThreadDispatcher.Execute(ExecutionMode, (this, task, (INavigationContext)state), s => s.Item1.ShowViewCallback(s.task, s.Item3, false));
        }

        private void ShowViewCallback(Task<IView> task, INavigationContext context, bool show)
        {
            try
            {
                UpdateView(task.Result, context);
                if (show)
                    ShowView(context);
                else
                    RefreshCallback(context);
            }
            catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is OperationCanceledException)
            {
                OnNavigationCanceled(context, ((OperationCanceledException)e.InnerExceptions[0]).CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                OnNavigationCanceled(context, e.CancellationToken);
            }
            catch (Exception e)
            {
                OnNavigationFailed(context, e);
            }
        }

        private void RefreshCallback(INavigationContext ctx)
        {
            try
            {
                if (!ActivateView(ctx))
                    OnNavigationCanceled(ctx, default);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ctx, e);
            }
        }

        private void CloseViewCallback(INavigationContext navigationContext)
        {
            try
            {
                if (_cancelArgs != null)
                    _cancelArgs.Cancel = false;
                else if (View != null)
                {
                    _shouldClose = true;
                    CloseView(navigationContext);
                }
            }
            catch (Exception e)
            {
                OnNavigationFailed(navigationContext, e);
            }
        }

        private void ClearFields(bool close)
        {
            if (close)
            {
                _closingContext = null;
                _shouldClose = false;
                _cancelArgs = null;
            }
            else
                _showingContext = null;
        }

        #endregion
    }
}