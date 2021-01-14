using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestSuspendableComponent : ISuspendable
    {
        public Func<object?, IReadOnlyMetadataContext?, ActionToken>? Suspend { get; set; }

        public bool IsSuspended { get; set; }

        ActionToken ISuspendable.Suspend(object? state, IReadOnlyMetadataContext? metadata) => Suspend?.Invoke(state, metadata) ?? default;
    }
}