using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class RawViewLifecycleDispatcher : ComponentDecoratorBase<IViewManager, IViewLifecycleDispatcherComponent>, IViewLifecycleDispatcherComponent
    {
        #region Constructors

        public RawViewLifecycleDispatcher(int priority = ComponentPriority.Max) : base(priority)
        {
        }

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
            view = MugenExtensions.Unwrap(view);
            foreach (var v in Owner.GetViews(view, metadata))
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