using System;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestLifecycleTrackerComponent<T> : ILifecycleTrackerComponent<T>, IHasPriority where T : class, IEnum
    {
        private readonly object? _owner;

        public TestLifecycleTrackerComponent(object? owner = null)
        {
            _owner = owner;
        }

        public Func<object, object, T, IReadOnlyMetadataContext?, bool>? IsInState { get; set; }

        public int Priority { get; set; }

        bool ILifecycleTrackerComponent<T>.IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(owner);
            return IsInState?.Invoke(owner, target, state, metadata) ?? false;
        }
    }
}