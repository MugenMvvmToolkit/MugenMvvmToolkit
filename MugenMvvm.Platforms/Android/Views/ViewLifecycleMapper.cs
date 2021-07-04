﻿using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class ViewLifecycleMapper : IViewLifecycleListener, IHasPriority
    {
        public int Priority { get; init; } = ViewComponentPriority.PostInitializer;

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == AndroidViewLifecycleState.Resuming)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Resumed)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Pausing)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappearing, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Paused)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Finished || lifecycleState == AndroidViewLifecycleState.Dismissed ||
                     lifecycleState == AndroidViewLifecycleState.DismissedAllowingStateLoss || lifecycleState == AndroidViewLifecycleState.Canceled ||
                     lifecycleState == AndroidViewLifecycleState.Destroyed && MugenExtensions.Unwrap(view) is IActivityView activity && activity.IsFinishing)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Closed, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Finishing || lifecycleState == AndroidViewLifecycleState.FinishingAfterTransition ||
                     lifecycleState == AndroidViewLifecycleState.Dismissing || lifecycleState == AndroidViewLifecycleState.DismissingAllowingStateLoss)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Closing, state, metadata);
        }
    }
}