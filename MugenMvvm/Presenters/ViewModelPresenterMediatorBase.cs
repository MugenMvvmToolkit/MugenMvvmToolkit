﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        private IViewMapping? _mapping;
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

        protected IViewMapping Mapping
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

        public void Initialize(IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            if (_viewModel == viewModel && _mapping == mapping)
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

            if (_closingContext != null)
                return GetPresenterResult(false, _closingContext?.Metadata ?? metadata);

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

        protected abstract void ShowView(TView view, INavigationContext context);

        protected abstract void InitializeView(TView view, INavigationContext context);

        protected abstract void CloseView(TView view, INavigationContext context);

        protected abstract void CleanupView(TView view, INavigationContext context);

        protected virtual bool ActivateView(TView view, INavigationContext context)
        {
            OnViewActivated();
            return true;
        }

        protected virtual void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual Task WaitBeforeShowAsync(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == state.NavigationType && callback.CallbackType == NavigationCallbackType.Showing, metadata);
        }

        protected virtual Task WaitBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
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

            var currentView = CurrentView;
            if (currentView != null)
                CleanupView(currentView, navigationContext);
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
            OnNavigated(_showingContext ?? GetNavigationContext(GetShowNavigationMode(CurrentView, null), null));
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

            _showingContext = navigationContext;
            if (navigationContext.NavigationMode.IsNew)
            {
                NavigationDispatcher.OnNavigating(navigationContext, (this, view, cancellationToken), (dispatcher, context, state) =>
                {
                    state.Item1.ViewManager
                        .InitializeAsync(state.Item1.Mapping, ViewModelViewRequest.GetRequestOrRaw(state.Item1.ViewModel, state.Item2), state.Item3, context.Metadata)
                        .ContinueWith(state.Item1.OnViewInitializedShowCallback!, context, TaskContinuationOptions.ExecuteSynchronously);
                    return false;
                }, (dispatcher, context, ex, state) => state.Item1._showingContext = null, cancellationToken);
            }
            else
            {
                if (CurrentView != null && (view == null || CurrentView == view))
                    ThreadDispatcher.Execute(ExecutionMode, RefreshCallback, navigationContext, metadata);
                else
                {
                    ViewManager
                        .InitializeAsync(Mapping, ViewModelViewRequest.GetRequestOrRaw(ViewModel, view), cancellationToken, metadata)
                        .ContinueWith((view == null ? OnViewInitializedShowCallback : (Action<Task<IView>, object>)OnViewInitializedRefreshCallback)!, navigationContext, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
        }

        protected virtual void CloseInternal(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _closingContext = GetNavigationContext(NavigationMode.Close, metadata);
            NavigationDispatcher.OnNavigating(_closingContext, this, (dispatcher, context, state) =>
            {
                state.ThreadDispatcher.Execute(state.ExecutionMode, state.CloseViewCallback, context);
                return false;
            }, (dispatcher, context, ex, state) => state._closingContext = null, cancellationToken);
        }

        [return: NotNullIfNotNull("view")]
        private TView? UpdateView(IView? view, INavigationContext context)
        {
            if (view == View)
                return (TView?)view?.Target;

            View = view;
            var oldView = CurrentView;
            var newView = view?.Target as TView ?? view?.Wrap<TView>(context.GetMetadataOrDefault(), _wrapperManager);
            if (oldView == newView)
                return oldView;

            if (oldView != null)
                CleanupView(oldView, context);
            CurrentView = newView;
            if (newView != null)
                InitializeView(newView, context);
            return newView;
        }

        private void OnViewInitializedShowCallback(Task<IView> task, object state)
        {
            if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
            {
                ShowViewCallback(task, (INavigationContext)state, true);
                return;
            }

            ThreadDispatcher.Execute(ExecutionMode, s =>
            {
                var tuple = (Tuple<ViewModelPresenterMediatorBase<TView>, Task<IView>, INavigationContext>)s!;
                tuple.Item1.ShowViewCallback(tuple.Item2, tuple.Item3, true);
            }, Tuple.Create(this, task, (INavigationContext)state));
        }

        private void OnViewInitializedRefreshCallback(Task<IView> task, object state)
        {
            if (ThreadDispatcher.CanExecuteInline(ExecutionMode))
            {
                ShowViewCallback(task, (INavigationContext)state, false);
                return;
            }

            ThreadDispatcher.Execute(ExecutionMode, s =>
            {
                var tuple = (Tuple<ViewModelPresenterMediatorBase<TView>, Task<IView>, INavigationContext>)s!;
                tuple.Item1.ShowViewCallback(tuple.Item2, tuple.Item3, false);
            }, Tuple.Create(this, task, (INavigationContext)state));
        }

        private void ShowViewCallback(Task<IView> task, INavigationContext context, bool show)
        {
            try
            {
                var view = UpdateView(task.Result, context);
                if (show)
                    ShowView(view, context);
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
                if (_cancelArgs != null)
                    _cancelArgs.Cancel = false;
                else if (CurrentView != null)
                {
                    _shouldClose = true;
                    CloseView(CurrentView, ctx);
                }
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