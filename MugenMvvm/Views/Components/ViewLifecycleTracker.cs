using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewLifecycleTracker : LifecycleTrackerBase<IViewManager, ViewLifecycleState, object>, IHasPriority, IViewLifecycleListener
    {
        public ViewLifecycleTracker() : this(null)
        {
        }

        public ViewLifecycleTracker(IAttachedValueManager? attachedValueManager) : base(attachedValueManager)
        {
            Trackers.Add(TrackViewState);
        }

        public int Priority { get; init; } = ViewComponentPriority.LifecycleTracker;

        public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            => OnLifecycleChanged(view.RawView, lifecycleState, metadata);

        protected override object GetTarget(object target) => MugenExtensions.Unwrap(target);

        private static void TrackViewState(object view, HashSet<ViewLifecycleState> states, ViewLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state.BaseState == ViewLifecycleState.Appeared)
            {
                states.Remove(ViewLifecycleState.Closed);
                states.Remove(ViewLifecycleState.Disappeared);
                states.Add(state.BaseState);
            }
            else if (state.BaseState == ViewLifecycleState.Closed || state.BaseState == ViewLifecycleState.Disappeared)
            {
                states.Remove(ViewLifecycleState.Appeared);
                states.Add(state.BaseState);
            }
        }
    }
}