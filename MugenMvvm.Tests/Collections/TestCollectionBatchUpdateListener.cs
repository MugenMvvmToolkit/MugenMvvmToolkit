using System;
using System.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Tests.Collections
{
    public class TestCollectionBatchUpdateListener : ICollectionBatchUpdateListener
    {
        public Action<ICollection, BatchUpdateType>? OnBeginBatchUpdate { get; set; }

        public Action<ICollection, BatchUpdateType>? OnEndBatchUpdate { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
        {
            if (OnBeginBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnBeginBatchUpdate?.Invoke(collection, batchUpdateType);
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
        {
            if (OnEndBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnEndBatchUpdate?.Invoke(collection, batchUpdateType);
        }
    }
}