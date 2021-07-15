using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class RawViewLifecycleDispatcher : ComponentDecoratorBase<IViewManager, IViewLifecycleListener>, IViewLifecycleListener
    {
        public RawViewLifecycleDispatcher(int priority = ViewComponentPriority.RawViewDispatcher) : base(priority)
        {
        }

        public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view.View != null)
            {
                Components.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
                return;
            }

            var hasView = false;
            foreach (var v in Owner.GetViews(view.SourceView, metadata))
            {
                hasView = true;
                Components.OnLifecycleChanged(viewManager, new ViewInfo(v), lifecycleState, state, metadata);
            }

            if (!hasView)
                Components.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }
    }
}