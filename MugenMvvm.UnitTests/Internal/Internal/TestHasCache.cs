using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestHasCache : IHasCache
    {
        public Action<object?, IReadOnlyMetadataContext?>? Invalidate { get; set; }

        void IHasCache.Invalidate(object? state, IReadOnlyMetadataContext? metadata) => Invalidate?.Invoke(state, metadata);
    }
}