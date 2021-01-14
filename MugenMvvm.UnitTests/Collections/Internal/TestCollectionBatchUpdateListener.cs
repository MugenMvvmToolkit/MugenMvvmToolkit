using System;
using System.Collections;
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

        public Action? OnBeginBatchUpdate { get; set; }

        public Action? OnEndBatchUpdate { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        void ICollectionBatchUpdateListener.OnBeginBatchUpdate(ICollection collection)
        {
            _collection.ShouldEqual(collection);
            if (OnBeginBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnBeginBatchUpdate?.Invoke();
        }

        void ICollectionBatchUpdateListener.OnEndBatchUpdate(ICollection collection)
        {
            _collection.ShouldEqual(collection);
            if (OnEndBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnEndBatchUpdate?.Invoke();
        }
    }
}