using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestSuspendableComponent<T> : ISuspendableComponent<T> where T : class
    {
        public Func<T, IReadOnlyMetadataContext?, ActionToken>? TrySuspend { get; set; }

        public Func<T, IReadOnlyMetadataContext?, bool>? IsSuspendedFunc { get; set; }

        public bool IsSuspended { get; set; }

        bool ISuspendableComponent<T>.IsSuspended(T owner, IReadOnlyMetadataContext? metadata) => IsSuspendedFunc?.Invoke(owner, metadata) ?? IsSuspended;

        ActionToken ISuspendableComponent<T>.TrySuspend(T owner, IReadOnlyMetadataContext? metadata) => TrySuspend?.Invoke(owner, metadata) ?? default;
    }
}