using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestSuspendableComponent : ISuspendable
    {
        #region Properties

        public bool IsSuspended { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, ActionToken>? Suspend { get; set; }

        #endregion

        #region Implementation of interfaces

        ActionToken ISuspendable.Suspend(object? state, IReadOnlyMetadataContext? metadata) => Suspend?.Invoke(state, metadata) ?? default;

        #endregion
    }
}