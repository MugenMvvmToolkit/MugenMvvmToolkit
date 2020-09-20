using System;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestLifecycleTrackerComponent<T> : ILifecycleTrackerComponent<T>
    {
        #region Properties

        public Func<object, object, T, IReadOnlyMetadataContext?, bool>? IsInState { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ILifecycleTrackerComponent<T>.IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata) => IsInState?.Invoke(owner, target, state, metadata) ?? false;

        #endregion
    }
}