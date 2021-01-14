using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using Should;

namespace MugenMvvm.UnitTests.Views.Internal
{
    public class TestViewLifecycleListener : IViewLifecycleListener, IHasPriority
    {
        private readonly IViewManager? _viewManager;

        public TestViewLifecycleListener(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        public Action<object, ViewLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IViewLifecycleListener.OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            OnLifecycleChanged?.Invoke(view, lifecycleState, state, metadata);
        }
    }
}