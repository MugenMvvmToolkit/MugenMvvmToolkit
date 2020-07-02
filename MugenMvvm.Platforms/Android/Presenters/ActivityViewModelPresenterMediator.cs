using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Views;
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
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
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
        private bool _shouldRaiseShown;
        private bool _shouldRaiseShownOnShowView;

        #endregion

        #region Constructors

        public ActivityViewModelPresenterMediator(IViewManager? viewManager = null, IWrapperManager? wrapperManager = null, INavigationDispatcher? navigationDispatcher = null, IThreadDispatcher? threadDispatcher = null)
            : base(viewManager, wrapperManager, navigationDispatcher, threadDispatcher)
        {
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Page;

        private ActivityViewDispatcher ActivityDispatcher
        {
            get
            {
                if (_activityDispatcher == null)
                    _activityDispatcher = new ActivityViewDispatcher(this);
                return _activityDispatcher;
            }
        }

        #endregion

        #region Methods

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
                    _shouldRaiseShown = true;
                }
                else //restore or first view
                    _shouldRaiseShownOnShowView = true;
            }
            else // refresh current
                _shouldRaiseShown = true;

            if (_shouldRaiseShown)
                ShowActivityIfNeed(metadata);
            base.ShowInternal(view, default, metadata);
        }

        protected override void ShowView(IActivityView view, INavigationContext context)
        {
            if (_shouldRaiseShownOnShowView)
            {
                OnViewShown();
                _shouldRaiseShownOnShowView = false;
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
            if (!Mapping.ViewType.IsInterface && typeof(IJavaObject).IsAssignableFrom(Mapping.ViewType))
                activityType = Class.FromType(Mapping.ViewType);
            if (Mapping is AndroidViewMapping m)
                resourceId = m.ResourceId;
            StartActivity(activityView, activityType, resourceId, flags, metadata);
        }

        protected virtual void StartActivity(IActivityView? topView, Class? activityType, int resourceId, int flags, IReadOnlyMetadataContext? metadata)
        {
            if (!MugenAndroidNativeService.StartActivity(topView!, activityType!, resourceId, flags))
                ExceptionManager.ThrowPresenterCannotShowRequest(Mapping, metadata);
        }

        protected virtual void OnResumed(IReadOnlyMetadataContext? metadata)
        {
            if (_shouldRaiseShown)
            {
                OnViewShown();
                _shouldRaiseShown = false;
            }
        }

        protected virtual void OnFinishing(ICancelableRequest cancelableRequest, IReadOnlyMetadataContext? metadata)
        {
            if (cancelableRequest.Cancel)
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
        }

        protected virtual void OnFinished(IReadOnlyMetadataContext? metadata)
        {
            OnViewClosed();
            ViewManager.RemoveComponent(ActivityDispatcher, metadata);
            _addedComponent = false;
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

            public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
            {
                if (view is IView v)
                    view = v.Target;
                if (lifecycleState == AndroidViewLifecycleState.Created && _mediator.Mapping.ViewType.IsInstanceOfType(view))
                {
                    _viewTask?.TrySetResult(view);
                    return;
                }

                var currentView = _mediator.CurrentView;
                if (currentView == null || !view.Equals(currentView))
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
                else if (lifecycleState == AndroidViewLifecycleState.Finishing || lifecycleState == AndroidViewLifecycleState.FinishingAfterTransition)
                {
                    if (!TypeChecker.IsValueType<TState>() && state is ICancelableRequest cancelableRequest)
                        _mediator.OnFinishing(cancelableRequest, metadata);
                }
            }

            public Task<IView>? TryInitializeAsync<TRequest>(IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                var viewModel = MugenExtensions.TryGetViewModelView(request, out object? view);
                if (_viewTask == null || !ReferenceEquals(viewModel, _mediator.ViewModel) || view != null)
                    return Components.TryInitializeAsync(mapping, request, cancellationToken, metadata);

                var tcs = new TaskCompletionSource<IView>();
                var valueTuple = (this, tcs, mapping, viewModel, metadata);
                _viewTask.Task.ContinueWith((task, o) =>
                {
                    var t = ((ActivityViewDispatcher @this, TaskCompletionSource<IView> tcs, IViewMapping mapping, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata))o;
                    t.@this._viewTask = null;
                    var result = t.@this.TryInitializeAsync(t.mapping, new ViewModelViewRequest(t.viewModel, task.Result), default, t.metadata);
                    if (result == null)
                        ExceptionManager.ThrowObjectNotInitialized(t.@this.Components);
                    t.tcs.TrySetFromTask(result);
                }, valueTuple, TaskContinuationOptions.ExecuteSynchronously);
                return tcs.Task;
            }

            public Task? TryCleanupAsync<TRequest>(IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            {
                return Components.TryCleanupAsync(view, request, cancellationToken, metadata);
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