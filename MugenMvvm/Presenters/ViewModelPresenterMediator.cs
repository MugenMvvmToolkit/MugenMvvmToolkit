using System;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Presenters
{
    public class ViewModelPresenterMediator<TView> : ViewModelPresenterMediatorBase<TView>, IViewLifecycleDispatcherComponent, IHasPriority where TView : class
    {
        #region Fields

        protected readonly IViewPresenter ViewPresenter;
        protected bool IsAppeared;
        protected bool LifecycleAdded;

        #endregion

        #region Constructors

        public ViewModelPresenterMediator(IViewModelBase viewModel, IViewMapping mapping, IViewPresenter viewPresenter,
            IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
            : base(viewModel, mapping, viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
        {
            Should.NotBeNull(viewPresenter, nameof(viewPresenter));
            ViewPresenter = viewPresenter;
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => ViewPresenter.NavigationType;

        public int Priority { get; set; } = ComponentPriority.Min;

        #endregion

        #region Implementation of interfaces

        void IViewLifecycleDispatcherComponent.OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                if (View == null && lifecycleState == ViewLifecycleState.Initializing && view is IView v &&
                    Equals(v.ViewModel, ViewModel) && v.Mapping.Id == Mapping.Id && !viewManager.IsInState(v.Target, ViewLifecycleState.Closed, metadata))
                    UpdateView(v, ShowingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata));

                if (View != null && Equals(View.Target, MugenExtensions.GetUnderlyingView(view)))
                    OnViewLifecycleChanged(lifecycleState, state, metadata);
            }
            catch (Exception e)
            {
                OnNavigationFailed(ShowingContext ?? ClosingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata), e);
            }
        }

        #endregion

        #region Methods

        protected override object GetViewRequest(object? view, INavigationContext navigationContext)
            => ViewPresenter.TryGetViewRequest(this, view, navigationContext) ?? base.GetViewRequest(view, navigationContext);

        protected override async Task ShowViewAsync(TView view, INavigationContext navigationContext)
        {
            var isAppeared = IsAppeared;
            await ViewPresenter.ShowAsync(this, view, navigationContext).ConfigureAwait(false);
            if (IsAppeared && isAppeared)
                OnViewShown(null);
        }

        protected override async Task<bool> ActivateViewAsync(TView view, INavigationContext navigationContext)
        {
            var isAppeared = IsAppeared;
            await ViewPresenter.ActivateAsync(this, view, navigationContext).ConfigureAwait(false);
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
            ViewPresenter.Initialize(this, view, navigationContext);
            if (ViewManager.IsInState(view, ViewLifecycleState.Appeared, meta))
                IsAppeared = true;
            else if (ViewManager.IsInState(view, ViewLifecycleState.Disappeared, meta))
                IsAppeared = false;
        }

        protected override Task CloseViewAsync(TView view, INavigationContext navigationContext) => ViewPresenter.CloseAsync(this, view, navigationContext);

        protected override void CleanupView(TView view, INavigationContext navigationContext) => ViewPresenter.Cleanup(this, view, navigationContext);

        protected internal override void OnViewClosed(IReadOnlyMetadataContext? metadata)
        {
            //close from lifecycle
            if (ClosingContext == null)
            {
                if (ShowingContext != null)
                {
                    TryClose(CurrentView, default, metadata);
                    return;
                }

                ClosingContext = GetNavigationContext(NavigationMode.Close, metadata);
                OnNavigating(ClosingContext);
            }

            if (!IsAppeared)
            {
                if (LifecycleAdded)
                {
                    LifecycleAdded = false;
                    ViewManager.RemoveComponent(this);
                }

                base.OnViewClosed(metadata);
            }
        }

        protected virtual void OnViewAppearing(object? state, IReadOnlyMetadataContext? metadata)
        {
            if (state is ICancelableRequest cancelableRequest && cancelableRequest.Cancel.GetValueOrDefault())
                return;
            if (ClosingContext != null)
                OnNavigationCanceled(ClosingContext, default);
            if (ShowingContext == null)
            {
                ShowingContext = GetNavigationContext(NavigationMode.Refresh, metadata);
                OnNavigating(ShowingContext);
            }
        }

        protected virtual void OnViewAppeared(object? state, IReadOnlyMetadataContext? metadata)
        {
            IsAppeared = true;
            if (ShowingContext != null)
                OnViewShown(metadata);
        }

        protected virtual void OnViewDisappeared(object? state, IReadOnlyMetadataContext? metadata)
        {
            IsAppeared = false;
            if (ClosingContext != null)
                OnViewClosed(metadata);
        }

        protected virtual void OnViewCleared(object? state, IReadOnlyMetadataContext? metadata) => UpdateView(null, ShowingContext ?? ClosingContext ?? GetNavigationContext(NavigationMode.Refresh, metadata));

        protected virtual void OnViewLifecycleChanged(ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewLifecycleState.Appearing)
                OnViewAppearing(state, metadata);
            else if (lifecycleState == ViewLifecycleState.Appeared)
                OnViewAppeared(state, metadata);
            else if (lifecycleState == ViewLifecycleState.Disappeared)
                OnViewDisappeared(state, metadata);
            else if (lifecycleState == ViewLifecycleState.Closing && state is ICancelableRequest cancelableRequest)
                OnViewClosing(cancelableRequest, metadata);
            else if (lifecycleState == ViewLifecycleState.Closed)
                OnViewClosed(metadata);
            else if (lifecycleState == ViewLifecycleState.Cleared)
                OnViewCleared(state, metadata);
        }

        #endregion
    }
}