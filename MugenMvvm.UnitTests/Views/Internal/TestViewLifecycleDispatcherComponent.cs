using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using Should;

namespace MugenMvvm.UnitTests.Views.Internal
{
    public class TestViewLifecycleDispatcherComponent : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public TestViewLifecycleDispatcherComponent(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Action<object, ViewLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IViewLifecycleDispatcherComponent.OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            _viewManager?.ShouldEqual(viewManager);
            OnLifecycleChanged?.Invoke(view, lifecycleState, state, metadata);
        }

        #endregion
    }
}