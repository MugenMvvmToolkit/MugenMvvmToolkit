using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class ViewLifecycleDispatcher : IViewLifecycleListener, ILifecycleTrackerComponent<ViewLifecycleState>, IHasPriority
    {
        private readonly IMugenApplication? _application;
        private readonly IPresenter? _presenter;

        public ViewLifecycleDispatcher(IPresenter? presenter = null, IMugenApplication? application = null)
        {
            _presenter = presenter;
            _application = application;
        }

        public bool FinishNotInitializedView { get; set; } = true;

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer - 1;

        public bool IsInState(object owner, object target, ViewLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            target = MugenExtensions.Unwrap(target);
            if (target is IActivityView activity)
            {
                if (state == AndroidViewLifecycleState.Finishing || state == AndroidViewLifecycleState.Finished)
                    return activity.IsFinishing;
                if (state == ViewLifecycleState.Closed)
                    return activity.IsFinishing || activity.IsDestroyed;
                if (state == AndroidViewLifecycleState.Destroyed)
                    return activity.IsDestroyed;
            }

            if (target is IFragmentView fragment)
            {
                if (state == ViewLifecycleState.Closed || state == AndroidViewLifecycleState.Destroyed)
                    return FragmentMugenExtensions.IsDestroyed(fragment);
            }

            return false;
        }

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if ((lifecycleState == AndroidViewLifecycleState.Started || lifecycleState == AndroidViewLifecycleState.Resumed) &&
                !viewManager.IsInState(view, AndroidViewLifecycleState.PendingInitialization, metadata) &&
                !viewManager.IsInState(view, AndroidViewLifecycleState.Finishing))
            {
                if (lifecycleState == AndroidViewLifecycleState.Started && view is not IView && viewManager.GetViews(view, metadata).IsEmpty)
                {
                    if (!_presenter.DefaultIfNull().TryShow(view, default, metadata).IsEmpty)
                        viewManager.OnLifecycleChanged(view, AndroidViewLifecycleState.PendingInitialization, state, metadata);
                }
                else if (lifecycleState == AndroidViewLifecycleState.Resumed && FinishNotInitializedView && view is IActivityView activityView &&
                         viewManager.GetViews(view).IsEmpty)
                    activityView.Finish();
            }

            if (lifecycleState == AndroidViewLifecycleState.Destroyed && view is IView v)
                viewManager.TryCleanupAsync(v, state, default, metadata);

            view = MugenExtensions.Unwrap(view);
            if (lifecycleState == AndroidViewLifecycleState.Starting && view is IActivityView && !_application.DefaultIfNull().IsInState(ApplicationLifecycleState.Activated))
                _application.DefaultIfNull().OnLifecycleChanged(ApplicationLifecycleState.Activating, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Started && view is IActivityView && !_application.DefaultIfNull().IsInState(ApplicationLifecycleState.Activated))
                _application.DefaultIfNull().OnLifecycleChanged(ApplicationLifecycleState.Activated, state, metadata);
        }
    }
}