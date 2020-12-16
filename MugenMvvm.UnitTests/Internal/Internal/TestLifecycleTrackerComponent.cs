using System;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestLifecycleTrackerComponent<T> : ILifecycleTrackerComponent<T>, IHasPriority
    {
        #region Fields

        private readonly object? _owner;

        #endregion

        #region Constructors

        public TestLifecycleTrackerComponent(object? owner = null)
        {
            _owner = owner;
        }

        #endregion

        #region Properties

        public Func<object, object, T, IReadOnlyMetadataContext?, bool>? IsInState { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ILifecycleTrackerComponent<T>.IsInState(object owner, object target, T state, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(owner);
            return IsInState?.Invoke(owner, target, state, metadata) ?? false;
        }

        #endregion
    }
}