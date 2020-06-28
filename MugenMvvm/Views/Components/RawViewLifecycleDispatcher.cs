using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Extensions.Internal;
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

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (view is IView)
            {
                Components.OnLifecycleChanged(view, lifecycleState, state, metadata);
                return;
            }

            var list = Owner.GetViews(view, metadata);
            var count = list.Count();
            if (count == 0)
                Components.OnLifecycleChanged(view, lifecycleState, state, metadata);
            else
            {
                for (var i = 0; i < count; i++)
                    Components.OnLifecycleChanged(list.Get(i), lifecycleState, state, metadata);
            }
        }

        #endregion
    }
}