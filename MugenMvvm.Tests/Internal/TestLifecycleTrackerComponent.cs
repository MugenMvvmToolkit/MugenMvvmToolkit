using System;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestLifecycleTrackerComponent<T> : ILifecycleTrackerComponent<T>, IHasPriority where T : class, IEnum
    {
        public Func<object, object, T, IReadOnlyMetadataContext?, bool>? IsInState { get; set; }

        public int Priority { get; set; }

        bool ILifecycleTrackerComponent<T>.IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata) =>
            IsInState?.Invoke(owner, target, state, metadata) ?? false;
    }
}