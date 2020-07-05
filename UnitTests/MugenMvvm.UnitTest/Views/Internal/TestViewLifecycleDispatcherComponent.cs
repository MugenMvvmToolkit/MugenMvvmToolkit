using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewLifecycleDispatcherComponent : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IViewManager, object, ViewLifecycleState, object?, Type, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        #endregion

        #region Implementation of interfaces

        void IViewLifecycleDispatcherComponent.OnLifecycleChanged<TState>(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            OnLifecycleChanged?.Invoke(viewManager, view, lifecycleState, state, typeof(TState), metadata);
        }

        #endregion
    }
}