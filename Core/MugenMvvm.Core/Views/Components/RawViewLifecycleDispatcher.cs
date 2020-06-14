using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class RawViewLifecycleDispatcher : AttachableComponentBase<IViewManager>, IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (view is IView)
                return;

            var list = Owner.GetViews(view, metadata);
            for (var i = 0; i < list.Count(); i++)
                Owner.OnLifecycleChanged(list.Get(i), lifecycleState, state, metadata);
        }

        #endregion
    }
}