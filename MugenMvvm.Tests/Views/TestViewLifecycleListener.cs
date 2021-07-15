using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Views;

namespace MugenMvvm.Tests.Views
{
    public class TestViewLifecycleListener : IViewLifecycleListener, IHasPriority
    {
        public Action<IViewManager, ViewInfo, ViewLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IViewLifecycleListener.
            OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata) =>
            OnLifecycleChanged?.Invoke(viewManager, view, lifecycleState, state, metadata);
    }
}