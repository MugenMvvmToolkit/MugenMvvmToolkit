using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestLifecycleTrackerComponent<TOwner, T> : ILifecycleTrackerComponent<TOwner, T>, IHasPriority
        where TOwner : class, IComponentOwner
        where T : class, IEnum
    {
        public Func<TOwner, object, T, IReadOnlyMetadataContext?, bool>? IsInState { get; set; }

        public int Priority { get; set; }

        bool ILifecycleTrackerComponent<TOwner, T>.IsInState(TOwner owner, object target, T state, IReadOnlyMetadataContext? metadata) =>
            IsInState?.Invoke(owner, target, state, metadata) ?? false;
    }
}