using System;
using System.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class TestCollectionBatchUpdateListener : ICollectionBatchUpdateListener
    {
        private readonly IObservableCollection _collection;

        public TestCollectionBatchUpdateListener(IObservableCollection collection)
        {
            _collection = collection;
        }

        public Action<BatchUpdateType>? OnBeginBatchUpdate { get; set; }

        public Action<BatchUpdateType>? OnEndBatchUpdate { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
        {
            _collection.ShouldEqual(collection);
            if (OnBeginBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnBeginBatchUpdate?.Invoke(batchUpdateType);
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(ICollection collection, BatchUpdateType batchUpdateType)
        {
            _collection.ShouldEqual(collection);
            if (OnEndBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnEndBatchUpdate?.Invoke(batchUpdateType);
        }
    }
}