using System;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestObservableCollectionBatchUpdateListener<T> : IObservableCollectionBatchUpdateListener<T>
    {
        #region Fields

        private readonly IObservableCollection<T> _collection;

        #endregion

        #region Constructors

        public TestObservableCollectionBatchUpdateListener(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<IObservableCollection<T>>? OnBeginBatchUpdate { get; set; }

        public Action<IObservableCollection<T>>? OnEndBatchUpdate { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        #endregion

        #region Implementation of interfaces

        void IObservableCollectionBatchUpdateListener<T>.OnBeginBatchUpdate(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnBeginBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnBeginBatchUpdate?.Invoke(collection);
        }

        void IObservableCollectionBatchUpdateListener<T>.OnEndBatchUpdate(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnEndBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnEndBatchUpdate?.Invoke(collection);
        }

        #endregion
    }
}