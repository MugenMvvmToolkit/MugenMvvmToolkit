using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.App.Internal
{
    public class TestApplicationLifecycleDispatcherComponent : IApplicationLifecycleDispatcherComponent, IHasPriority
    {
        #region Properties

        public Action<IMugenApplication, ApplicationLifecycleState, object?, Type, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IApplicationLifecycleDispatcherComponent.OnLifecycleChanged<TState>(IMugenApplication application, ApplicationLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            OnLifecycleChanged?.Invoke(application, lifecycleState, state, typeof(TState), metadata);
        }

        #endregion
    }
}