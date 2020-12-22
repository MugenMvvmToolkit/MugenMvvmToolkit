using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewLifecycleTracker : LifecycleTrackerBase<ViewLifecycleState, object>, IHasPriority, IViewLifecycleListener
    {
        #region Constructors

        public ViewLifecycleTracker()
        {
            Trackers.Add(TrackViewState);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.LifecycleTracker;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            => OnLifecycleChanged(view, lifecycleState, metadata);

        #endregion

        #region Methods

        private static void TrackViewState(object view, HashSet<ViewLifecycleState> states, ViewLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state == ViewLifecycleState.Appeared)
            {
                states.Remove(ViewLifecycleState.Closed);
                states.Remove(ViewLifecycleState.Disappeared);
                states.Add(state);
            }
            else if (state == ViewLifecycleState.Closed || state == ViewLifecycleState.Disappeared)
            {
                states.Remove(ViewLifecycleState.Appeared);
                states.Add(state);
            }
        }

        protected override object GetTarget(object target) => MugenExtensions.Unwrap(target);

        #endregion
    }
}