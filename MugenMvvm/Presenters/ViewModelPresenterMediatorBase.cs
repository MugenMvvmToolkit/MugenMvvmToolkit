using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Presenters
{
    public abstract class ViewModelPresenterMediatorBase<TView> : IViewModelPresenterMediator, INavigationProvider, IHasNavigationInfo
        where TView : class
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly IViewManager? _viewManager;
        private readonly IWrapperManager? _wrapperManager;

        protected ICancelableRequest? ClosingCancelArgs;
        protected INavigationContext? ClosingContext;
        protected INavigationContext? ShowingContext;

        private string _id;
        private string? _navigationId;

        #endregion

        #region Constructors

        protected ViewModelPresenterMediatorBase(IViewModelBase viewModel, IViewMapping mapping, IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            ViewModel = viewModel;
            Mapping = mapping;
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _navigationDispatcher = navigationDispatcher;
            _threadDispatcher = threadDispatcher;
            _id = $"{GetType().FullName}{mapping.Id}";
        }

        #endregion

        #region Properties

        public string Id
        {
            get => _id;
            protected set
            {
                if (_id == value)
                    return;
                Should.NotBeNullOrEmpty(value, nameof(value));
                if (_navigationId != null)
                    ExceptionManager.ThrowObjectInitialized(this, nameof(NavigationId));
                _id = value;
            }
        }

        public abstract NavigationType NavigationType { get; }

        public string NavigationId => _navigationId ??= this.GetNavigationId(ViewModel);

        public IViewMapping Mapping { get; }

        public IViewModelBase ViewModel { get; }

        public IView? View { get; private set; }

        protected TView? CurrentView { get; private set; }

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected IWrapperManager WrapperManager => _wrapperManager.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected virtual ThreadExecutionMode ExecutionMode => ThreadExecutionMode.Main;

        #endregion

        #region Implementation of interfaces

        public virtual IPresenterResult? TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (ShowingContext != null)
                return GetPresenterResult(false, ShowingContext.Metadata ?? metadata);

            WaitBeforeShowAsync(view, cancellationToken, metadata).ContinueWithEx((this, view, cancellationToken, metadata), (_, s) =>
            {
                try
                {
                    s.Item1.ShowInternal(s.view, s.cancellationToken, s.metadata);
                }
                catch (Exception e)
                {
                    s.Item1.OnNavigationFailed(s.Item1.GetNavigationContext(s.Item1.GetShowNavigationMode(s.view, s.metadata), s.metadata), e);
                }
            });

            return GetPresenterResult(true, metadata);
        }

        public virtual IPresenterResult? TryClose(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (View == null)
                return null;

            if (ClosingContext != null)
                return GetPresenterResult(false, ClosingContext.Metadata ?? metadata);

            WaitBeforeCloseAsync(cancellationToken, metadata).ContinueWithEx((this, cancellationToken, metadata), (_, s) =>
            {
                try
                {
                    s.Item1.CloseInternal(s.cancellationToken, s.metadata);
                }
                catch (Exception e)
                {
                    s.Item1.OnNavigationFailed(s.Item1.GetNavigationContext(NavigationMode.Close, s.metadata), e);
                }
            });
            return GetPresenterResult(false, metadata);
        }

        #endregion

        #region Methods

        protected abstract void ShowView(TView view, INavigationContext navigationContext);

        protected abstract void InitializeView(TView view, INavigationContext navigationContext);

        protected abstract void CloseView(TView view, INavigationContext navigationContext);

        protected abstract void CleanupView(TView view, INavigationContext navigationContext);

        protected virtual bool ActivateView(TView view, INavigationContext navigationContext)
        {
            OnViewActivated(navigationContext.GetMetadataOrDefault());
            return true;
        }

        protected virtual Task WaitBeforeShowAsync(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => Task.CompletedTask;

        protected virtual Task WaitBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => Task.CompletedTask;

        protected virtual IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata) => new PresenterResult(ViewModel, NavigationId, this, NavigationType, metadata);

        protected virtual INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata) =>
            NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationId, NavigationType, mode, metadata);

        protected virtual object GetViewRequest(object? view, INavigationContext navigationContext) => ViewModelViewRequest.GetRequestOrRaw(ViewModel, view);

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

        protected virtual void OnNavigating(INavigationContext navigationContext)
        {
            NavigationDispatcher.OnNavigating(navigationContext);
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

            if (navigationContext.NavigationMode.IsClose)
                UpdateView(null, navigationContext);
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

        protected virtual void ShowInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
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

            ShowingContext = navigationContext;
            if (navigationContext.NavigationMode.IsNew)
            {
                NavigationDispatcher.OnNavigatingAsync(navigationContext, (this, view, cancellationToken), (dispatcher, context, state) =>
                {
                    state.Item1.ViewManager
                        .InitializeAsync(state.Item1.Mapping, state.Item1.GetViewRequest(state.view, context), state.cancellationToken, context.Metadata)
                        .ContinueWith(state.Item1.OnViewInitializedShowCallback!, context, TaskContinuationOptions.ExecuteSynchronously);
                    return false;
                }, (dispatcher, context, ex, state) => state.Item1.ShowingContext = null, cancellationToken);
            }
            else
            {
                if (CurrentView != null && (view == null || Equals(CurrentView, view)))
                    ThreadDispatcher.Execute(ExecutionMode, RefreshCallback, navigationContext, metadata);
                else
                {
                    ViewManager
                        .InitializeAsync(Mapping, GetViewRequest(view, navigationContext), cancellationToken, metadata)
                        .ContinueWith(view == null ? OnViewInitializedShowCallback : (Action<Task<IView>, object>)OnViewInitializedRefreshCallback, navigationContext, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
        }

        protected virtual void CloseInternal(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ClosingContext = GetNavigationContext(NavigationMode.Close, metadata);
            NavigationDispatcher.OnNavigatingAsync(ClosingContext, this, (dispatcher, context, state) =>
            {
                state.ThreadDispatcher.Execute(state.ExecutionMode, state.CloseViewCallback, context);
                return false;
            }, (dispatcher, context, ex, state) => state.ClosingContext = null, cancellationToken);
        }

        protected internal virtual void OnViewShown(IReadOnlyMetadataContext? metadata)
        {
            if (View != null)
                OnNavigated(ShowingContext ?? GetNavigationContext(GetShowNavigationMode(CurrentView, null), metadata));
        }

        protected internal virtual void OnViewActivated(IReadOnlyMetadataContext? metadata)
        {
            if (View != null)
                OnNavigated(ShowingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata));
        }

        protected internal virtual void OnViewClosing(ICancelableRequest e, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (View == null || e.Cancel.GetValueOrDefault(false))
                    return;

                ClosingCancelArgs = e;
                if (ClosingContext == null)
                {
                    e.Cancel = true;
                    CloseInternal(default, metadata);
                }
                else
                    e.Cancel = false;
            }
            finally
            {
                ClosingCancelArgs = null;
            }
        }

        protected internal virtual void OnViewClosed(IReadOnlyMetadataContext? metadata)
        {
            if (View != null)
                OnNavigated(ClosingContext ?? GetNavigationContext(NavigationMode.Close, metadata));
        }

        [return: NotNullIfNotNull("view")]
        protected TView? UpdateView(IView? view, INavigationContext navigationContext)
        {
            if (view == View || Equals(view?.Target, View?.Target))
                return CurrentView;

            var oldView = View;
            var oldViewObj = CurrentView;
            var newView = view?.Target as TView ?? view?.Wrap<TView>(navigationContext.GetMetadataOrDefault(), _wrapperManager);

            if (oldViewObj != null)
                CleanupView(oldViewObj, navigationContext);
            if (oldView != null)
                ViewManager.CleanupAsync(oldView, navigationContext, default, navigationContext.GetMetadataOrDefault());

            View = view;
            CurrentView = newView;
            if (newView != null)
                InitializeView(newView, navigationContext);
            return newView;
        }

        private void OnViewInitializedShowCallback(Task<IView> task, object state) => OnViewInitializedCallback(task, state, true);

        private void OnViewInitializedRefreshCallback(Task<IView> task, object state) => OnViewInitializedCallback(task, state, false);

        private void OnViewInitializedCallback(Task<IView> task, object state, bool show)
        {
            if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
            {
                ShowViewCallback(task, (INavigationContext)state, show);
                return;
            }

            ThreadDispatcher.Execute(ExecutionMode, s =>
            {
                var tuple = (Tuple<ViewModelPresenterMediatorBase<TView>, Task<IView>, INavigationContext, bool>)s!;
                tuple.Item1.ShowViewCallback(tuple.Item2, tuple.Item3, tuple.Item4);
            }, Tuple.Create(this, task, (INavigationContext)state, show));
        }

        private void ShowViewCallback(Task<IView> task, INavigationContext navigationContext, bool show)
        {
            try
            {
                var view = UpdateView(task.Result, navigationContext);
                if (show)
                    ShowView(view, navigationContext);
                else
                    RefreshCallback(navigationContext);
            }
            catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is OperationCanceledException)
            {
                OnNavigationCanceled(navigationContext, ((OperationCanceledException)e.InnerExceptions[0]).CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                OnNavigationCanceled(navigationContext, e.CancellationToken);
            }
            catch (Exception e)
            {
                OnNavigationFailed(navigationContext, e);
            }
        }

        private void RefreshCallback(object? state)
        {
            var ctx = (INavigationContext)state!;
            try
            {
                if (CurrentView == null || !ActivateView(CurrentView, ctx))
                    OnNavigationCanceled(ctx, default);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ctx, e);
            }
        }

        private void CloseViewCallback(object? state)
        {
            var ctx = (INavigationContext)state!;
            try
            {
                if (ClosingCancelArgs != null)
                    ClosingCancelArgs.Cancel = false;
                else if (CurrentView != null)
                    CloseView(CurrentView, ctx);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ctx, e);
            }
        }

        private void ClearFields(bool close)
        {
            if (close)
            {
                ClosingContext = null;
                ClosingCancelArgs = null;
            }
            else
                ShowingContext = null;
        }

        #endregion
    }
}