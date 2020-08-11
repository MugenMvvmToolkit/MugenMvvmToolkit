using System.Threading;
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
        protected bool ShouldRaiseOnAppeared;
        protected bool ShouldRaiseOnDisappeared;

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
            if (view is IView v)
                view = v.Target;

            if (View != null && Equals(View?.Target, view))
                OnViewLifecycleChanged(lifecycleState, state, metadata);
        }

        #endregion

        #region Methods

        protected override Task WaitBeforeShowAsync(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => ViewPresenter.WaitBeforeShowAsync(this, view, cancellationToken, metadata);

        protected override Task WaitBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => ViewPresenter.WaitBeforeCloseAsync(this, cancellationToken, metadata);

        protected override object GetViewRequest(object? view, INavigationContext navigationContext)
            => ViewPresenter.TryGetViewRequest(this, view, navigationContext) ?? base.GetViewRequest(view, navigationContext);

        protected override void ShowView(TView view, INavigationContext navigationContext)
        {
            ShouldRaiseOnAppeared = true;
            ViewPresenter.Show(this, view, navigationContext);
            if (IsAppeared && ShouldRaiseOnAppeared)
            {
                OnViewShown();
                ShouldRaiseOnAppeared = false;
            }
        }

        protected override bool ActivateView(TView view, INavigationContext navigationContext)
        {
            ShouldRaiseOnAppeared = true;
            if (!ViewPresenter.Activate(this, view, navigationContext))
            {
                ShouldRaiseOnAppeared = false;
                return base.ActivateView(view, navigationContext);
            }

            return true;
        }

        protected override void InitializeView(TView view, INavigationContext navigationContext)
        {
            if (!LifecycleAdded)
            {
                LifecycleAdded = true;
                ViewManager.AddComponent(this);
            }
        }

        protected override void CloseView(TView view, INavigationContext navigationContext) => ViewPresenter.Close(this, view, navigationContext);

        protected override void CleanupView(TView view, INavigationContext navigationContext)
        {
        }

        protected internal override void OnViewClosed()
        {
            if (IsAppeared)
            {
                ShouldRaiseOnDisappeared = true;
                return;
            }

            LifecycleAdded = false;
            IsAppeared = false;
            ShouldRaiseOnDisappeared = false;
            ShouldRaiseOnAppeared = false;
            ViewManager.RemoveComponent(this);
            base.OnViewClosed();
        }

        protected virtual void OnViewAppeared(object? state, IReadOnlyMetadataContext? metadata)
        {
            IsAppeared = true;
            if (ShouldRaiseOnAppeared)
            {
                ShouldRaiseOnAppeared = false;
                OnViewShown();
            }
        }

        protected virtual void OnViewDisappeared(object? state, IReadOnlyMetadataContext? metadata)
        {
            IsAppeared = false;
            if (ShouldRaiseOnDisappeared)
                OnViewClosed();
        }

        protected virtual void OnViewLifecycleChanged(ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewLifecycleState.Appeared)
                OnViewAppeared(state, metadata);
            else if (lifecycleState == ViewLifecycleState.Disappeared)
                OnViewDisappeared(state, metadata);
            else if (lifecycleState == ViewLifecycleState.Closing && state is ICancelableRequest cancelableRequest)
                OnViewClosing(cancelableRequest);
            else if (lifecycleState == ViewLifecycleState.Closed)
                OnViewClosed();
        }

        #endregion
    }
}