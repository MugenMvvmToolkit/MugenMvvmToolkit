using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionChangingListener<T> : ICollectionChangingListener<T>, IHasPriority
    {
        #region Fields

        private readonly object _collection;

        #endregion

        #region Constructors

        public TestCollectionChangingListener(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<T, int>? OnAdding { get; set; }

        public Action<T, T, int>? OnReplacing { get; set; }

        public Action<T, int, int>? OnMoving { get; set; }

        public Action<T, int>? OnRemoving { get; set; }

        public Action<IEnumerable<T>?>? OnResetting { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICollectionChangingListener<T>.OnAdding(IReadOnlyCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdding == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdding?.Invoke(item!, index);
        }

        void ICollectionChangingListener<T>.OnReplacing(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplacing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplacing?.Invoke(oldItem!, newItem!, index);
        }

        void ICollectionChangingListener<T>.OnMoving(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoving?.Invoke(item!, oldIndex, newIndex);
        }

        void ICollectionChangingListener<T>.OnRemoving(IReadOnlyCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoving?.Invoke(item!, index);
        }

        void ICollectionChangingListener<T>.OnResetting(IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            _collection.ShouldEqual(collection);
            if (OnResetting == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnResetting?.Invoke(items);
        }

        #endregion
    }
}