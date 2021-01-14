using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewLifecycleAwareViewModelHandler : IViewLifecycleListener, IHasPriority
    {
        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view is IView v)
                (v.ViewModel as IViewLifecycleAwareViewModel)?.OnViewLifecycleChanged(v, lifecycleState, state, metadata);
        }
    }
}