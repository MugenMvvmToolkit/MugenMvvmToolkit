using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewStateMapperDispatcher : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IMugenApplication? _application;

        #endregion

        #region Constructors

        public AndroidViewStateMapperDispatcher(IMugenApplication? application = null)
        {
            _application = application;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view is IView v)
            {
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
                view = v.Target;
            }

            if (lifecycleState == AndroidViewLifecycleState.Starting && view is IActivityView && _application.DefaultIfNull().IsInBackground(true))
                _application.DefaultIfNull().OnLifecycleChanged(ApplicationLifecycleState.Activating, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Started && view is IActivityView && _application.DefaultIfNull().IsInBackground(true))
                _application.DefaultIfNull().OnLifecycleChanged(ApplicationLifecycleState.Activated, state, metadata);
        }

        #endregion
    }
}