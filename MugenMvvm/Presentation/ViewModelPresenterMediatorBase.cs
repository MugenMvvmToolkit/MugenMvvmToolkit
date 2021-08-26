using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Presentation
{
    public abstract class ViewModelPresenterMediatorBase<TView> : IViewModelPresenterMediator, INavigationProvider, IHasNavigationInfo
        where TView : class
    {
        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly IViewModelManager? _viewModelManager;
        private readonly IViewManager? _viewManager;
        private readonly IWrapperManager? _wrapperManager;
        private readonly object _locker;
        private CancellationTokenSource? _showingToken;
        private TaskCompletionSource<object?>? _showingTask;

        private string _id;
        private string? _navigationId;

        protected ViewModelPresenterMediatorBase(IViewModelBase viewModel, IViewMapping mapping, IViewManager? viewManager = null, IWrapperManager? wrapperManager = null,
            INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null, IViewModelManager? viewModelManager = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            ViewModel = viewModel;
            Mapping = mapping;
            _viewManager = viewManager;
            _wrapperManager = wrapperManager;
            _navigationDispatcher = navigationDispatcher;
            _threadDispatcher = threadDispatcher;
            _viewModelManager = viewModelManager;
            _id = $"{GetType().FullName}{mapping.Id}";
            _locker = new object();
        }

        public abstract NavigationType NavigationType { get; }

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

        public string NavigationId => _navigationId ??= this.GetNavigationId(ViewModel);

        public IViewMapping Mapping { get; }

        public IViewModelBase ViewModel { get; }

        public IView? View { get; private set; }

        protected virtual ThreadExecutionMode ExecutionMode => ThreadExecutionMode.Main;

        protected bool IsClosing => ClosingContext != null;

        protected bool IsShowing => ShowingContext != null;

        protected bool IsShown { get; private set; }

        protected INavigationContext? ShowingContext { get; set; }

        protected INavigationContext? ClosingContext { get; set; }

        protected ICancelableRequest? ClosingCancelArgs { get; set; }

        protected TView? CurrentView { get; private set; }

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        protected IWrapperManager WrapperManager => _wrapperManager.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher.DefaultIfNull();

        protected IViewModelManager ViewModelManager => _viewModelManager.DefaultIfNull();

        public IPresenterResult? TryShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            lock (_locker)
            {
                if (!CanShow(view, cancellationToken, metadata))
                    return null;

                if (ShowingContext != null)
                    return GetPresenterResult(false, ShowingContext.GetMetadataOrDefault(metadata));

                Show(view, cancellationToken, metadata);
                return GetPresenterResult(true, metadata);
            }
        }

        public IPresenterResult? TryClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            lock (_locker)
            {
                if (!IsShowing && !IsShown)
                    return null;

                if (!CanClose(view, cancellationToken, metadata))
                    return null;

                if (ClosingContext != null)
                    return GetPresenterResult(false, ClosingContext.GetMetadataOrDefault(metadata));

                Close(view, cancellationToken, metadata);
                return GetPresenterResult(false, metadata);
            }
        }

        protected internal virtual void OnViewShown(IReadOnlyMetadataContext? metadata)
        {
            if (View != null)
            {
                ShowingContext ??= GetShowNavigationContext(null, metadata);
                OnNavigated(ShowingContext);
            }
        }

        protected internal virtual void OnViewActivated(IReadOnlyMetadataContext? metadata)
        {
            if (View != null)
            {
                ShowingContext ??= GetShowNavigationContext(null, metadata);
                OnNavigated(ShowingContext);
            }
        }

        protected internal virtual void OnViewClosing(ICancelableRequest e, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (View == null || e.Cancel.GetValueOrDefault(false))
                    return;

                ClosingCancelArgs = e;
                lock (_locker)
                {
                    if (ClosingContext == null)
                    {
                        e.Cancel = true;
                        Close(CurrentView, default, metadata);
                    }
                    else
                        e.Cancel = false;
                }
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

        protected abstract Task ShowViewAsync(TView view, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected abstract Task CloseViewAsync(TView view, INavigationContext navigationContext, CancellationToken cancellationToken);

        protected abstract void InitializeView(TView view, INavigationContext navigationContext);

        protected abstract void CleanupView(TView view, INavigationContext navigationContext);

        protected virtual ValueTask<bool> ActivateViewAsync(TView view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<bool>(cancellationToken).AsValueTask();
            OnViewActivated(navigationContext.GetMetadataOrDefault());
            return new ValueTask<bool>(true);
        }

        protected virtual IPresenterResult GetPresenterResult(bool show, IReadOnlyMetadataContext? metadata) =>
            new PresenterResult(ViewModel, NavigationId, this, NavigationType, metadata);

        protected virtual INavigationContext GetShowNavigationContext(object? view, IReadOnlyMetadataContext? metadata)
        {
            NavigationMode? mode = null;
            if (IsShown)
                mode = NavigationMode.Refresh;
            else
            {
                var hashSet = ViewModel.GetOrDefault(InternalMetadata.OpenedNavigationProviders);
                if (hashSet != null)
                {
                    lock (hashSet)
                    {
                        if (hashSet.Contains(Id))
                            mode = NavigationMode.Restore;
                    }
                }
            }

            var context = GetNavigationContext(mode ?? NavigationMode.New, metadata);
            if (view != null)
                context.Metadata.Set(InternalMetadata.View, view);
            return context;
        }

        protected virtual INavigationContext GetNavigationContext(NavigationMode mode, IReadOnlyMetadataContext? metadata) =>
            NavigationDispatcher.GetNavigationContext(ViewModel, this, NavigationId, NavigationType, mode, metadata);

        protected virtual bool CanShow(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => CheckNavigationType(metadata);

        protected virtual bool CanClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => CheckNavigationType(metadata) && View != null && (view == null || Equals(View.Target, view));

        protected virtual object GetViewRequest(object? view, INavigationContext navigationContext) => ViewModelViewRequest.GetRequestOrRaw(ViewModel, view);

        protected virtual void OnNavigating(INavigationContext navigationContext) => NavigationDispatcher.OnNavigating(navigationContext);

        protected virtual void OnNavigated(INavigationContext navigationContext)
        {
            var hashSet = ViewModel.Metadata.GetOrAdd(InternalMetadata.OpenedNavigationProviders, this, (_, __, ___) => new HashSet<string>(StringComparer.Ordinal));
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
            exception = exception.TryGetBaseException(out var canceledException);
            if (canceledException == null)
            {
                ClearFields(navigationContext, null, exception);
                NavigationDispatcher.OnNavigationFailed(navigationContext, exception);
            }
            else
                OnNavigationCanceled(navigationContext, canceledException.CancellationToken);
        }

        protected virtual void OnNavigationCanceled(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            ClearFields(navigationContext, cancellationToken, null);
            NavigationDispatcher.OnNavigationCanceled(navigationContext, cancellationToken);
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
                ViewManager.TryCleanupAsync(oldView, navigationContext, default, navigationContext.GetMetadataOrDefault());

            View = view;
            CurrentView = newView;
            if (newView != null)
                InitializeView(newView, navigationContext);
            return newView;
        }

        protected Task WaitShowAsync() => _showingTask?.Task ?? Task.CompletedTask;

        private async void Show(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            INavigationContext? context = null;
            try
            {
                _showingTask?.TrySetCanceled();
                _showingToken.SafeCancel();
                _showingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
                _showingTask = new TaskCompletionSource<object?>();
                cancellationToken = _showingToken.Token;

                context = GetShowNavigationContext(view, metadata);
                ShowingContext = context;

                await ThreadDispatcher.SwitchToAsync(ExecutionMode);
                if (!EnsureValidState(context, cancellationToken))
                    return;

                if (!CanShow(view, cancellationToken, metadata))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                if (!await NavigationDispatcher.OnNavigatingAsync(context, cancellationToken))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (!context.NavigationMode.IsNew && CurrentView != null && (view == null || Equals(CurrentView, view)))
                {
                    await RefreshAsync(context, cancellationToken);
                    return;
                }

                var newView = await ViewManager.InitializeAsync(Mapping, GetViewRequest(view, context), cancellationToken, context.GetMetadataOrDefault());
                cancellationToken.ThrowIfCancellationRequested();

                var currentView = UpdateView(newView, context);
                if (context.NavigationMode.IsNew || view == null)
                    await ShowViewAsync(currentView, context, cancellationToken);
                else
                    await RefreshAsync(context, cancellationToken);
                IsShown = true;
            }
            catch (Exception e)
            {
                OnNavigationFailed(context ?? GetNavigationContext(NavigationMode.Refresh, metadata), e);
            }
        }

        private async void Close(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            INavigationContext? context = null;
            try
            {
                _showingToken.SafeCancel();
                context = GetNavigationContext(NavigationMode.Close, metadata);
                ClosingContext = context;

                await WaitShowAsync();
                await ThreadDispatcher.SwitchToAsync(ExecutionMode);

                if (!EnsureValidState(context, cancellationToken))
                    return;

                if (!CanClose(view, cancellationToken, metadata))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                if (!await NavigationDispatcher.OnNavigatingAsync(context, cancellationToken))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (ClosingCancelArgs != null)
                    ClosingCancelArgs.Cancel = false;
                else if (CurrentView != null)
                    await CloseViewAsync(CurrentView, context, cancellationToken);
            }
            catch (Exception e)
            {
                OnNavigationFailed(context ?? GetNavigationContext(NavigationMode.Close, metadata), e);
            }
        }

        private async Task RefreshAsync(INavigationContext context, CancellationToken cancellationToken)
        {
            if (CurrentView == null || !await ActivateViewAsync(CurrentView, context, cancellationToken))
                OnNavigationCanceled(context, default);
        }

        private void ClearFields(INavigationContext navigationContext, CancellationToken? cancellationToken, Exception? error)
        {
            lock (_locker)
            {
                if (View != null && (cancellationToken != null || error != null) &&
                    (navigationContext.NavigationMode.IsNew || navigationContext.NavigationMode.IsRestore ||
                     Equals(navigationContext.GetOrDefault(InternalMetadata.View), View.Target)))
                    UpdateView(null, navigationContext);
                _showingToken?.Dispose();
                _showingToken = null;
                if (navigationContext.NavigationMode.IsClose)
                {
                    if (cancellationToken == null && error == null)
                        IsShown = false;
                    ClosingContext = null;
                    ClosingCancelArgs = null;
                }
                else
                {
                    ShowingContext = null;
                    var tcs = _showingTask;
                    _showingTask = null;
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
        }

        private bool CheckNavigationType(IReadOnlyMetadataContext? metadata) =>
            metadata == null || !metadata.TryGet(NavigationMetadata.NavigationType, out var type) || type == NavigationType;

        private bool EnsureValidState(INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (!navigationContext.NavigationMode.IsClose && ViewModel.IsInState(ViewModelLifecycleState.Disposed, navigationContext.GetMetadataOrDefault(), _viewModelManager))
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
    }
}