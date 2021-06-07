using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.App
{
    public class TestApplicationLifecycleListener : IApplicationLifecycleListener, IHasPriority
    {
        public Action<IMugenApplication, ApplicationLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IApplicationLifecycleListener.OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata) =>
            OnLifecycleChanged?.Invoke(application, lifecycleState, state, metadata);
    }
}