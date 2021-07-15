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
        public int Priority { get; init; } = ViewComponentPriority.PostInitializer;

        public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata) =>
            (view.View?.ViewModel as IViewLifecycleAwareViewModel)?.OnViewLifecycleChanged(view.View!, lifecycleState, state, metadata);
    }
}