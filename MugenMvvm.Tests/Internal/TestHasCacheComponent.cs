using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Tests.Internal
{
    public class TestHasCacheComponent<T> : IHasCacheComponent<T> where T : class
    {
        public Action<T, object?, IReadOnlyMetadataContext?>? Invalidate { get; set; }

        void IHasCacheComponent<T>.Invalidate(T owner, object? state, IReadOnlyMetadataContext? metadata) => Invalidate?.Invoke(owner, state, metadata);
    }
}