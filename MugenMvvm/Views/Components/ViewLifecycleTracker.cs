using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewLifecycleTracker : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            (view as IView)?.Metadata.Set(ViewMetadata.LifecycleState, lifecycleState, out _);
        }

        #endregion
    }
}