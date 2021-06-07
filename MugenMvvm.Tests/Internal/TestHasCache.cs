using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Internal
{
    public class TestHasCache : IHasCache
    {
        public Action<object, object?, IReadOnlyMetadataContext?>? Invalidate { get; set; }

        void IHasCache.Invalidate(object sender, object? state, IReadOnlyMetadataContext? metadata) => Invalidate?.Invoke(sender,state, metadata);
    }
}