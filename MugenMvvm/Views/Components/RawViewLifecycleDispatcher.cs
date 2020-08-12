using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class RawViewLifecycleDispatcher : ComponentDecoratorBase<IViewManager, IViewLifecycleDispatcherComponent>, IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Max;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view is IView)
            {
                Components.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
                return;
            }

            var hasView = false;
            foreach (var v in Owner.GetViews(view, metadata).Iterator())
            {
                hasView = true;
                Components.OnLifecycleChanged(viewManager, v, lifecycleState, state, metadata);
            }

            if (!hasView)
                Components.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }

        #endregion
    }
}