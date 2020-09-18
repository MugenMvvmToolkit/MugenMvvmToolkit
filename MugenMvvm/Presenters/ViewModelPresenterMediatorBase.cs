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
using MugenMvvm.Internal;
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
            ShowInternal(view, ShowingCancellationTokenSource.Token, metadata);
            return GetPresenterResult(true, metadata);
        }

        public IPresenterResult? TryClose(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!CanClose(view, cancellationToken, metadata))
                return null;

            if (ClosingContext != null)
                return GetPresenterResult(false, ClosingContext.GetMetadataOrDefault(metadata));

            CloseInternal(view, cancellationToken, metadata);
            ShowingCancellationTokenSource?.Cancel();
            return GetPresenterResult(false, metadata);
        }

        #endregion

        #region Methods

        protected abstract Task ShowViewAsync(TView view, INavigationContext navigationContext);

        protected abstract Task CloseViewAsync(TView view, INavigationContext navigationContext);

        protected abstract void InitializeView(TView view, INavigationContext navigationContext);

        protected abstract void CleanupView(TView view, INavigationContext navigationContext);

        protected virtual Task<bool> ActivateViewAsync(TView view, INavigationContext navigationContext)
        {
            OnViewActivated(navigationContext.GetMetadataOrDefault());
            return Default.TrueTask;
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

        protected virtual async void ShowInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            INavigationContext? context = null;
            try
            {
                context = GetNavigationContext(GetShowNavigationMode(view, metadata), metadata);
                ShowingContext = context;
                if (!EnsureValidState(context, cancellationToken))
                    return;

                if (!CanShow(view, cancellationToken, metadata))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }


                if (!await NavigationDispatcher.OnNavigatingAsync(context, cancellationToken).ConfigureAwait(false))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                if (!context.NavigationMode.IsNew && CurrentView != null && (view == null || Equals(CurrentView, view)))
                {
                    if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
                        RefreshCallback(context);
                    else
                        ThreadDispatcher.Execute(ExecutionMode, RefreshCallback, context);
                    return;
                }

                var newView = await ViewManager.InitializeAsync(Mapping, GetViewRequest(view, context), cancellationToken, context.GetMetadataOrDefault()).ConfigureAwait(false);
                bool isShowRequest = context.NavigationMode.IsNew || view == null;
                if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
                {
                    ShowViewCallback(newView, context, isShowRequest);
                    return;
                }

                ThreadDispatcher.Execute(ExecutionMode, s =>
                {
                    var tuple = (Tuple<ViewModelPresenterMediatorBase<TView>, IView, INavigationContext, bool>)s!;
                    tuple.Item1.ShowViewCallback(tuple.Item2, tuple.Item3, tuple.Item4);
                }, Tuple.Create(this, newView, context, isShowRequest));
            }
            catch (Exception e)
            {
                OnNavigationFailed(context ?? GetNavigationContext(NavigationMode.Refresh, metadata), e);
            }
        }

        protected virtual async void CloseInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            INavigationContext? context = null;
            try
            {
                if (ShowingTaskCompletionSource != null)
                    await ShowingTaskCompletionSource.Task.ConfigureAwait(false);

                context = GetNavigationContext(NavigationMode.Close, metadata);
                ClosingContext = context;
                if (!EnsureValidState(context, cancellationToken))
                    return;

                if (!CanClose(view, cancellationToken, metadata))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                if (!await NavigationDispatcher.OnNavigatingAsync(context, cancellationToken).ConfigureAwait(false))
                {
                    OnNavigationCanceled(context, cancellationToken);
                    return;
                }

                if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
                    CloseViewCallback(context);
                else
                    ThreadDispatcher.Execute(ExecutionMode, CloseViewCallback, context);
            }
            catch (Exception e)
            {
                OnNavigationFailed(context ?? GetNavigationContext(NavigationMode.Close, metadata), e);
            }
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
                ViewManager.TryCleanupAsync(oldView, navigationContext, default, navigationContext.GetMetadataOrDefault());

            View = view;
            CurrentView = newView;
            if (newView != null)
                InitializeView(newView, navigationContext);
            return newView;
        }

        private async void ShowViewCallback(IView newView, INavigationContext navigationContext, bool show)
        {
            try
            {
                var view = UpdateView(newView, navigationContext);
                if (show)
                    await ShowViewAsync(view, navigationContext).ConfigureAwait(false);
                else
                    RefreshCallback(navigationContext);
            }
            catch (Exception e)
            {
                OnNavigationFailed(navigationContext, e);
            }
        }

        private async void RefreshCallback(object? state)
        {
            var ctx = (INavigationContext)state!;
            try
            {
                if (CurrentView == null || !await ActivateViewAsync(CurrentView, ctx).ConfigureAwait(false))
                    OnNavigationCanceled(ctx, default);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ctx, e);
            }
        }

        private async void CloseViewCallback(object? state)
        {
            var ctx = (INavigationContext)state!;
            try
            {
                if (ClosingCancelArgs != null)
                    ClosingCancelArgs.Cancel = false;
                else if (CurrentView != null)
                    await CloseViewAsync(CurrentView, ctx).ConfigureAwait(false);
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