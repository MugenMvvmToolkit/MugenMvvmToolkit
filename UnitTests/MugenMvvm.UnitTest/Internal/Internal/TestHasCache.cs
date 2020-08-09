using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestHasCache : IHasCache
    {
        #region Properties

        public Action<object?, IReadOnlyMetadataContext?>? Invalidate { get; set; }

        #endregion

        #region Implementation of interfaces

        void IHasCache.Invalidate(object? state, IReadOnlyMetadataContext? metadata) => Invalidate?.Invoke(state, metadata);

        #endregion
    }
}