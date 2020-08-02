using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Presenters
{
    public class ActivityViewModelPresenterMediator : ViewModelPresenterMediatorBase<IActivityView>
    {
        #region Fields

        private ActivityViewDispatcher? _activityDispatcher;
        private bool _addedComponent;
        private bool _shouldRaiseOnResume;
        private bool _shouldRaiseOnShow;
        private bool _ignoreFinishingNavigation;

        #endregion

        #region Constructors

        public ActivityViewModelPresenterMediator(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
            : base(viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
        {
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Page;

        private ActivityViewDispatcher ActivityDispatcher => _activityDispatcher ??= new ActivityViewDispatcher(this);

        #endregion

        #region Methods

        protected override Task WaitBeforeCloseAsync(CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close, metadata);
        }

        protected override void ShowInternal(object? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!_addedComponent)
            {
                ViewManager.AddComponent(ActivityDispatcher, metadata);
                _addedComponent = true;
            }

            if (View == null)
            {
                //new activity pending navigation
                if (view == null)
                {
                    ActivityDispatcher.WaitView();
                    _shouldRaiseOnResume = true;
                }
                else //restore or first view
                    _shouldRaiseOnShow = true;
            }
            else // refresh current
                _shouldRaiseOnResume = true;

            if (_shouldRaiseOnResume && view == null)
                ShowActivityIfNeed(metadata);
            base.ShowInternal(view, default, metadata);
        }

        protected override void ShowView(IActivityView view, INavigationContext context)
        {
            if (_shouldRaiseOnShow)
            {
                OnViewShown();
                _shouldRaiseOnShow = false;
            }
        }

        protected override bool ActivateView(IActivityView view, INavigationContext context)
        {
            return true;
        }

        protected override void InitializeView(IActivityView view, INavigationContext context)
        {
        }

        protected override void CloseView(IActivityView view, INavigationContext context)
        {
            view.Finish();
        }

        protected override void CleanupView(IActivityView view, INavigationContext context)
        {
        }

        protected virtual void ShowActivityIfNeed(IReadOnlyMetadataContext? metadata)
        {
            var clearBackStack = metadata != null && metadata.Get(NavigationMetadata.ClearBackStack);
            if (clearBackStack)
                ViewManager.OnLifecycleChanged(this, AndroidViewLifecycleState.ClearBackStack, CurrentView, metadata);

            var flags = 0;
            var activityView = NavigationDispatcher.GetTopView<IActivityView>(NavigationType);
            if (CurrentView != null)
            {
                if (activityView == CurrentView)
                    return;
                flags = (int)ActivityFlags.ReorderToFront;
            }

            Class? activityType = null;
            var resourceId = 0;
            if (typeof(Object).IsAssignableFrom(Mapping.ViewType))
                activityType = Class.FromType(Mapping.ViewType);
            if (Mapping is IAndroidViewMapping m)
                resourceId = m.ResourceId;
            StartActivity(activityView, activityType, resourceId, flags, metadata);
        }

        protected virtual void StartActivity(IActivityView? topView, Class? activityType, int resourceId, int flags, IReadOnlyMetadataContext? metadata)
        {
            if (!ActivityExtensions.StartActivity(topView!, activityType!, resourceId, flags))
                ExceptionManager.ThrowPresenterCannotShowRequest(Mapping, metadata);
        }

        protected virtual void OnResumed(IReadOnlyMetadataContext? metadata)
        {
            if (_shouldRaiseOnResume)
            {
                OnViewShown();
                _shouldRaiseOnResume = false;
            }
        }

        protected virtual void OnFinishing(bool afterTransition, ICancelableRequest cancelableRequest, IReadOnlyMetadataContext? metadata)
        {
            if (cancelableRequest.Cancel || _ignoreFinishingNavigation)
                return;

            var currentView = CurrentView;
            if (currentView == null)
                return;

            if (cancelableRequest is CancelEventArgs args)
                OnViewClosing(currentView, args);
            else
            {
                var eventArgs = new CancelEventArgs(cancelableRequest.Cancel);
                OnViewClosing(currentView, eventArgs);
                cancelableRequest.Cancel = eventArgs.Cancel;
            }

            if (afterTransition && !cancelableRequest.Cancel)
                _ignoreFinishingNavigation = true;
        }

        protected virtual void OnFinished(IReadOnlyMetadataContext? metadata)
        {
            OnViewClosed();
            ViewManager.RemoveComponent(ActivityDispatcher, metadata);
            _addedComponent = false;
            _ignoreFinishingNavigation = false;
        }

        #endregion

        #region Nested types

        private sealed class ActivityViewDispatcher : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IViewLifecycleDispatcherComponent, IHasPriority
        {
            #region Fields

            private readonly ActivityViewModelPresenterMediator _mediator;
            private TaskCompletionSource<object>? _viewTask;

            #endregion

            #region Constructors

            public ActivityViewDispatcher(ActivityViewModelPresenterMediator mediator)
            {
                _mediator = mediator;
            }

            #endregion

            #region Properties

            public int Priority => ViewComponentPriority.ViewModelViewProviderDecorator + 1;

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                if (view is IView v)
                    view = v.Target;
                if (lifecycleState == AndroidViewLifecycleState.Created && _mediator.Mapping.ViewType.IsInstanceOfType(view))
                {
                    _viewTask?.TrySetResult(view);
                    return;
                }

                var currentView = _mediator.CurrentView;
                if (currentView == null || !Equals(view, currentView))
                {
                    if (lifecycleState == AndroidViewLifecycleState.ClearBackStack && _mediator != view)
                    {
                        _mediator.OnFinished(metadata);
                        currentView?.Finish();
                    }

                    return;
                }

                if (lifecycleState == AndroidViewLifecycleState.Resumed)
                    _mediator.OnResumed(metadata);
                else if (lifecycleState == AndroidViewLifecycleState.Finished || lifecycleState == AndroidViewLifecycleState.Destroyed && currentView.IsFinishing)
                    _mediator.OnFinished(metadata);
                else if ((lifecycleState == AndroidViewLifecycleState.Finishing || lifecycleState == AndroidViewLifecycleState.FinishingAfterTransition) && state is ICancelableRequest cancelableRequest)
                    _mediator.OnFinishing(lifecycleState == AndroidViewLifecycleState.FinishingAfterTransition, cancelableRequest, metadata);
            }

            public Task<IView>? TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
                if (_viewTask == null || viewModel != _mediator.ViewModel || view != null)
                    return Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);

                var tcs = new TaskCompletionSource<IView>();
                _viewTask.Task.ContinueWithEx((this, viewManager, tcs, mapping, viewModel, metadata), (task, s) =>
                {
                    s.Item1._viewTask = null;
                    var result = s.Item1.TryInitializeAsync(s.viewManager, s.mapping, new ViewModelViewRequest(s.viewModel, task.Result), default, s.metadata);
                    if (result == null)
                        ExceptionManager.ThrowObjectNotInitialized(s.Item1.Components);
                    s.tcs.TrySetFromTask(result);
                });
                return tcs.Task;
            }

            public Task? TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                return Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);
            }

            #endregion

            #region Methods

            public void WaitView()
            {
                _viewTask = new TaskCompletionSource<object>();
            }

            #endregion
        }

        #endregion
    }
}