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
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public sealed class ViewLifecycleDispatcher : IViewLifecycleListener, ILifecycleTrackerComponent<IViewManager, ViewLifecycleState>, IHasPriority
    {
        private readonly IMugenApplication? _application;
        private readonly IPresenter? _presenter;

        public ViewLifecycleDispatcher(IPresenter? presenter = null, IMugenApplication? application = null)
        {
            _presenter = presenter;
            _application = application;
        }

        public bool FinishNotInitializedView { get; set; } = true;

        public int Priority { get; init; } = ViewComponentPriority.PostInitializer - 1;

        public bool IsInState(IViewManager owner, object target, ViewLifecycleState state, IReadOnlyMetadataContext? metadata)
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

        public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if ((lifecycleState == AndroidViewLifecycleState.Started || lifecycleState == AndroidViewLifecycleState.Resumed) &&
                !viewManager.IsInState(view.RawView, AndroidViewLifecycleState.PendingInitialization, metadata) &&
                !viewManager.IsInState(view.RawView, AndroidViewLifecycleState.Finishing))
            {
                if (lifecycleState == AndroidViewLifecycleState.Started && view.View == null && viewManager.GetViews(view.SourceView, metadata).IsEmpty)
                {
                    if (!_presenter.DefaultIfNull(view.RawView).TryShow(view.RawView, default, metadata).IsEmpty)
                        viewManager.OnLifecycleChanged(view, AndroidViewLifecycleState.PendingInitialization, state, metadata);
                }
                else if (lifecycleState == AndroidViewLifecycleState.Resumed && FinishNotInitializedView && view.RawView is IActivityView activityView &&
                         viewManager.GetViews(view.SourceView).IsEmpty)
                    activityView.Finish();
            }

            if (lifecycleState == AndroidViewLifecycleState.Destroyed && view.View != null)
                viewManager.TryCleanupAsync(view.View, state, default, metadata);

            if (lifecycleState == AndroidViewLifecycleState.Starting && view.Is<IActivityView>() && !_application.DefaultIfNull().IsInState(ApplicationLifecycleState.Activated))
                _application.DefaultIfNull().OnLifecycleChanged(ApplicationLifecycleState.Activating, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Started && view.Is<IActivityView>() && !_application.DefaultIfNull().IsInState(ApplicationLifecycleState.Activated))
                _application.DefaultIfNull().OnLifecycleChanged(ApplicationLifecycleState.Activated, state, metadata);
        }
    }
}