using MugenMvvm.Android.Enums;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewStateMapperDispatcher : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (!(view is IView v))
                return;

            if (lifecycleState == AndroidViewLifecycleState.Resuming)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appearing, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Resumed)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Appeared, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Pausing)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappearing, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Paused)
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Disappeared, state, metadata);
            if (lifecycleState == AndroidViewLifecycleState.Destroyed)
                viewManager.TryCleanupAsync(v, state, default, metadata);
        }

        #endregion
    }
}