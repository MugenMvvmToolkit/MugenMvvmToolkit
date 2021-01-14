using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.App.Internal
{
    public class TestApplicationLifecycleListener : IApplicationLifecycleListener, IHasPriority
    {
        private readonly IMugenApplication? _owner;

        public TestApplicationLifecycleListener(IMugenApplication? owner = null)
        {
            _owner = owner;
        }

        public Action<ApplicationLifecycleState, object?, IReadOnlyMetadataContext?>? OnLifecycleChanged { get; set; }

        public int Priority { get; set; }

        void IApplicationLifecycleListener.OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(application);
            OnLifecycleChanged?.Invoke(lifecycleState, state, metadata);
        }
    }
}