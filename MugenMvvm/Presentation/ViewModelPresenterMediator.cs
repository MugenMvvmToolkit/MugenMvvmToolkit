using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Views;

namespace MugenMvvm.Presentation
{
    public class ViewModelPresenterMediator<TView> : ViewModelPresenterMediatorBase<TView>, IViewLifecycleListener, IHasPriority where TView : class
    {
        private bool _isClosingFromLifecycle;

        public ViewModelPresenterMediator(IViewModelBase viewModel, IViewMapping mapping, IViewPresenterMediator viewPresenterMediator,
            IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null,
            IThreadDispatcher? threadDispatcher = null, IViewModelManager? viewModelManager = null)
            : base(viewModel, mapping, viewManager, wrapperManager, navigationDispatcher, threadDispatcher, viewModelManager)
        {
            Should.NotBeNull(viewPresenterMediator, nameof(viewPresenterMediator));
            ViewPresenterMediator = viewPresenterMediator;
        }

        public override NavigationType NavigationType => ViewPresenterMediator.NavigationType;

        public IViewPresenterMediator ViewPresenterMediator { get; }

        public int Priority { get; init; } = ComponentPriority.Min;

        protected bool IsAppeared { get; set; }

        protected bool LifecycleAdded { get; set; }

        protected internal override void OnViewClosed(NavigationMode navigationMode, IReadOnlyMetadataContext? metadata)
        {
            //close from lifecycle
            if (ClosingContext == null)
            {
                _isClosingFromLifecycle = true;
                if (ShowingContext != null)
                {
                    TryClose(CurrentView, default, metadata);
                    return;
                }

                ClosingContext = GetNavigationContext(navigationMode, metadata);
                OnNavigating(ClosingContext);
            }

            if (!IsAppeared)
            {
                _isClosingFromLifecycle = false;
                if (LifecycleAdded)
                {
                    LifecycleAdded = false;
                    ViewManager.RemoveComponent(this);
                }

                base.OnViewClosed(navigationMode, metadata);
            }
        }

        protected virtual void OnViewAppearing(object? state, IReadOnlyMetadataContext? metadata)
        {
            var cancelableRequest = state as ICancelableRequest;
            if (cancelableRequest != null && cancelableRequest.Cancel.GetValueOrDefault())
            {
                if (ShowingContext != null)
                    OnNavigationCanceled(ShowingContext, default);
                return;
            }

            if (_isClosingFromLifecycle && ClosingContext != null)
                OnNavigationCanceled(ClosingContext, default);

            if (ClosingContext != null && cancelableRequest != null)
            {
                if (ShowingContext != null)
                {
                    cancelableRequest.Cancel = true;
                    OnNavigationCanceled(ShowingContext, default);
                }
            }
            else if (ShowingContext == null)
            {
                ShowingContext = GetNavigationContext(NavigationMode.Refresh, metadata);
                OnNavigating(ShowingContext);
            }
        }

        protected virtual void OnViewAppeared(object? state, IReadOnlyMetadataContext? metadata)
        {
            _isClosingFromLifecycle = false;
            IsAppeared = true;
            if (ShowingContext != null)
                OnViewShown(metadata);
            if (ClosingContext == null && IsDisposed(metadata))
                TryClose(CurrentView, default, metadata);
        }

        protected virtual void OnViewDisappeared(object? state, IReadOnlyMetadataContext? metadata)
        {
            IsAppeared = false;
            if (ClosingContext != null)
                OnViewClosed(NavigationMode.Close, metadata);
        }

        protected virtual void OnViewCleared(object? state, IReadOnlyMetadataContext? metadata)
        {
            if (!IsClosing)
                UpdateView(null, ShowingContext ?? ClosingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata));
        }

        protected virtual void OnViewLifecycleChanged(ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState.BaseState == ViewLifecycleState.Appearing)
                OnViewAppearing(state, metadata);
            else if (lifecycleState.BaseState == ViewLifecycleState.Appeared)
                OnViewAppeared(state, metadata);
            else if (lifecycleState.BaseState == ViewLifecycleState.Disappeared)
                OnViewDisappeared(state, metadata);
            else if (lifecycleState.BaseState == ViewLifecycleState.Closing && state is ICancelableRequest cancelableRequest)
            {
                if (ClosingContext == null)
                    _isClosingFromLifecycle = true;
                OnViewClosing(lifecycleState.NavigationMode ?? NavigationMode.Close, cancelableRequest, metadata);
            }
            else if (lifecycleState.BaseState == ViewLifecycleState.Closed)
                OnViewClosed(lifecycleState.NavigationMode ?? NavigationMode.Close, metadata);
            else if (lifecycleState.BaseState == ViewLifecycleState.Cleared)
                OnViewCleared(state, metadata);
        }

        protected override object GetViewRequest(object? view, INavigationContext navigationContext)
            => ViewPresenterMediator.TryGetViewRequest(this, view, navigationContext) ?? base.GetViewRequest(view, navigationContext);

        protected override async Task ShowViewAsync(TView view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var isAppeared = IsAppeared;
            await ViewPresenterMediator.ShowAsync(this, view, navigationContext, cancellationToken);
            if (IsAppeared && isAppeared)
                OnViewShown(null);
        }

        protected override async ValueTask<bool> ActivateViewAsync(TView view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var isAppeared = IsAppeared;
            await ViewPresenterMediator.ActivateAsync(this, view, navigationContext, cancellationToken);
            if (IsAppeared && isAppeared)
                OnViewActivated(null);
            return true;
        }

        protected override void InitializeView(TView view, INavigationContext navigationContext)
        {
            if (!LifecycleAdded)
            {
                LifecycleAdded = true;
                ViewManager.AddComponent(this);
            }

            var meta = navigationContext.GetMetadataOrDefault();
            if (ViewManager.IsInState(view, ViewLifecycleState.Closed, meta))
            {
                ExceptionManager.ThrowCanceledException();
                return;
            }

            ViewPresenterMediator.Initialize(this, view, navigationContext);
            if (ViewManager.IsInState(view, ViewLifecycleState.Appeared, meta))
                IsAppeared = true;
            else if (ViewManager.IsInState(view, ViewLifecycleState.Disappeared, meta))
                IsAppeared = false;
        }

        protected override Task CloseViewAsync(TView view, INavigationContext navigationContext, CancellationToken cancellationToken) =>
            ViewPresenterMediator.CloseAsync(this, view, navigationContext, cancellationToken);

        protected override void CleanupView(TView view, INavigationContext navigationContext) => ViewPresenterMediator.Cleanup(this, view, navigationContext);

        void IViewLifecycleListener.OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (View == null && lifecycleState.BaseState == ViewLifecycleState.Initializing && view.View != null &&
                    Equals(view.View.ViewModel, ViewModel) && view.View.Mapping.Id == Mapping.Id && !viewManager.IsInState(view.View.Target, ViewLifecycleState.Closed, metadata))
                    UpdateView(view.View, ShowingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata));

                if (View != null && view.IsSameView(View))
                    OnViewLifecycleChanged(lifecycleState, state, metadata);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ShowingContext ?? ClosingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata), e);
            }
        }
    }
}