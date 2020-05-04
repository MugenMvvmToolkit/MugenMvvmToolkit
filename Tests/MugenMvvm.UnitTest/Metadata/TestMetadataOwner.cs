﻿using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Metadata
{
    public class TestMetadataOwner<T> : IMetadataOwner<T> where T : class, IReadOnlyMetadataContext
    {
        #region Properties

        public bool HasMetadata { get; set; }

        public T Metadata { get; set; } = null!;

        #endregion
    }
}