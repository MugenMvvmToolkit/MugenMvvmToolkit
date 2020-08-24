using System;
using System.Collections;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionBatchUpdateListener : ICollectionBatchUpdateListener
    {
        #region Fields

        private readonly IObservableCollection _collection;

        #endregion

        #region Constructors

        public TestCollectionBatchUpdateListener(IObservableCollection collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action? OnBeginBatchUpdate { get; set; }

        public Action? OnEndBatchUpdate { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        #endregion

        #region Implementation of interfaces

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

        #endregion
    }
}