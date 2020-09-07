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

        private string _id;
        private string? _navigationId;

        protected ICancelableRequest? ClosingCancelArgs;
        protected INavigationContext? ClosingContext;
        protected CancellationTokenSource? ShowingCancellationTokenSource;
        protected INavigationContext? ShowingContext;
        protected TaskCompletionSource<object?>? ShowingTaskCompletionSource;

        #endregion

        #region Constructors

        protected ViewModelPresenterMediatorBase(IViewModelBase viewModel, IViewMapping mapping, IViewManager? viewManager = null, IWrapperManager? wrapperManager = null,
            INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
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

        public IPresenterResult? TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!CanShow(view, cancellationToken, metadata))
                return null;

            if (ShowingContext != null)
                return GetPresenterResult(false, ShowingContext.GetMetadataOrDefault(metadata));

            ShowingTaskCompletionSource?.TrySetCanceled();
            ShowingCancellationTokenSource?.Cancel();
            ShowingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
            ShowingTaskCompletionSource = new TaskCompletionSource<object?>();
            try
            {
                ShowInternal(view, ShowingCancellationTokenSource.Token, metadata);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ShowingContext ?? GetNavigationContext(GetShowNavigationMode(view, metadata), metadata), e);
            }

            return GetPresenterResult(true, metadata);
        }

        public IPresenterResult? TryClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!CanClose(view, cancellationToken, metadata))
                return null;

            if (ClosingContext != null)
                return GetPresenterResult(false, ClosingContext.GetMetadataOrDefault(metadata));

            ShowingCancellationTokenSource?.Cancel();
            (ShowingTaskCompletionSource?.Task ?? Task.CompletedTask).ContinueWithEx((this, view, cancellationToken, metadata), (_, s) =>
            {
                try
                {
                    s.Item1.CloseInternal(s.view, s.cancellationToken, s.metadata);
                }
                catch (Exception e)
                {
                    s.Item1.OnNavigationFailed(s.Item1.ClosingContext ?? s.Item1.GetNavigationContext(NavigationMode.Close, s.metadata), e);
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

        protected virtual IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata) => new PresenterResult(ViewModel, NavigationId, this, NavigationType, metadata);

        protected virtual INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata) =>
            NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationId, NavigationType, mode, metadata);

        protected virtual bool CanShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => CheckNavigationType(metadata);

        protected virtual bool CanClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => CheckNavigationType(metadata) && View != null && (view == null || Equals(View.Target, view));

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

        protected virtual void OnNavigating(INavigationContext navigationContext) => NavigationDispatcher.OnNavigating(navigationContext);

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

            ClearFields(navigationContext, null, null);
            NavigationDispatcher.OnNavigated(navigationContext);

            if (navigationContext.NavigationMode.IsClose)
                UpdateView(null, navigationContext);
        }

        protected virtual void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            if (exception.TryGetCanceledException(out var canceledException))
                OnNavigationCanceled(navigationContext, canceledException.CancellationToken);
            else
            {
                ClearFields(navigationContext, null, exception);
                NavigationDispatcher.OnNavigationFailed(navigationContext, exception);
            }
        }

        protected virtual void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            ClearFields(navigationContext, cancellationToken, null);
            NavigationDispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
        }

        protected virtual void ShowInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ShowingContext = GetNavigationContext(GetShowNavigationMode(view, metadata), metadata);
            if (!EnsureValidState(ShowingContext, cancellationToken))
                return;

            if (!CanShow(view, cancellationToken, metadata))
            {
                OnNavigationCanceled(ShowingContext, cancellationToken);
                return;
            }

            NavigationDispatcher
                .OnNavigatingAsync(ShowingContext, cancellationToken)
                .ContinueWithEx((this, ShowingContext, view, cancellationToken), (task, state) => state.Item1.OnNavigatingShowCallback(task, state.ShowingContext, state.view, state.cancellationToken));
        }

        protected virtual void CloseInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ClosingContext = GetNavigationContext(NavigationMode.Close, metadata);
            if (!EnsureValidState(ClosingContext, cancellationToken))
                return;

            if (!CanClose(view, cancellationToken, metadata))
            {
                OnNavigationCanceled(ClosingContext, cancellationToken);
                return;
            }

            NavigationDispatcher
                .OnNavigatingAsync(ClosingContext, cancellationToken)
                .ContinueWithEx((this, ClosingContext, cancellationToken), (task, state) => state.Item1.OnNavigatingCloseCallback(task, state.ClosingContext, state.cancellationToken));
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
                    CloseInternal(CurrentView, default, metadata);
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
            var newView = view?.Target as TView ?? view?.Wrap<TView>(navigationContext.GetMetadataOrDefault(), WrapperManager);

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

        private void OnNavigatingShowCallback(Task<bool> task, INavigationContext context, object? view, CancellationToken cancellationToken)
        {
            try
            {
                if (!task.Result)
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                if (!context.NavigationMode.IsNew && CurrentView != null && (view == null || Equals(CurrentView, view)))
                {
                    ThreadDispatcher.Execute(ExecutionMode, RefreshCallback, context, context.GetMetadataOrDefault());
                    return;
                }

                ViewManager
                    .InitializeAsync(Mapping, GetViewRequest(view, context), cancellationToken, context.GetMetadataOrDefault())
                    .ContinueWith((context.NavigationMode.IsNew || view == null ? OnViewInitializedShowCallback : (Action<Task<IView>, object>)OnViewInitializedRefreshCallback)!, context,
                        TaskContinuationOptions.ExecuteSynchronously);
            }
            catch (Exception e)
            {
                OnNavigationFailed(context, e);
            }
        }

        private void OnNavigatingCloseCallback(Task<bool> task, INavigationContext context, CancellationToken cancellationToken)
        {
            try
            {
                if (task.Result)
                    ThreadDispatcher.Execute(ExecutionMode, CloseViewCallback, context);
                else
                    OnNavigationCanceled(context, cancellationToken);
            }
            catch (Exception e)
            {
                OnNavigationFailed(context, e);
            }
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

        private void ClearFields(INavigationContext navigationContext, CancellationToken? cancellationToken, Exception? error)
        {
            if (navigationContext.NavigationMode.IsNew && (cancellationToken != null || error != null))
                UpdateView(null, navigationContext);
            ShowingCancellationTokenSource?.Dispose();
            ShowingCancellationTokenSource = null;
            if (navigationContext.NavigationMode.IsClose)
            {
                ClosingContext = null;
                ClosingCancelArgs = null;
            }
            else
            {
                ShowingContext = null;
                var tcs = ShowingTaskCompletionSource;
                ShowingTaskCompletionSource = null;
                if (tcs == null)
                    return;
                if (error != null)
                    tcs.TrySetException(error);
                else if (cancellationToken != null)
                    tcs.TrySetCanceled(cancellationToken.Value);
                else
                    tcs.TrySetResult(null);
            }
        }

        private bool CheckNavigationType(IReadOnlyMetadataContext? metadata) => metadata == null || !metadata.TryGet(NavigationMetadata.NavigationType, out var type) || type == NavigationType;

        private bool EnsureValidState(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (ViewModel.IsDisposed())
            {
                OnNavigationFailed(navigationContext, new ObjectDisposedException(ViewModel.GetType().FullName));
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                OnNavigationCanceled(navigationContext, cancellationToken);
                return false;
            }

            return true;
        }

        #endregion
    }
}