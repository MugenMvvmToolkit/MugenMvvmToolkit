using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections
{
    public class TestObservableCollectionChangingListener<T> : IObservableCollectionChangingListener<T>, IHasPriority
    {
        #region Fields

        private readonly IObservableCollection<T> _collection;

        #endregion

        #region Constructors

        public TestObservableCollectionChangingListener(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<IObservableCollection<T>, T, int>? OnAdding { get; set; }

        public Action<IObservableCollection<T>, T, T, int>? OnReplacing { get; set; }

        public Action<IObservableCollection<T>, T, int, int>? OnMoving { get; set; }

        public Action<IObservableCollection<T>, T, int>? OnRemoving { get; set; }

        public Action<IObservableCollection<T>, IEnumerable<T>>? OnResetting { get; set; }

        public Action<IObservableCollection<T>>? OnClearing { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IObservableCollectionChangingListener<T>.OnAdding(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdding == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdding?.Invoke(collection, item, index);
        }

        void IObservableCollectionChangingListener<T>.OnReplacing(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplacing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplacing?.Invoke(collection, oldItem, newItem, index);
        }

        void IObservableCollectionChangingListener<T>.OnMoving(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoving?.Invoke(collection, item, oldIndex, newIndex);
        }

        void IObservableCollectionChangingListener<T>.OnRemoving(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoving?.Invoke(collection, item, index);
        }

        void IObservableCollectionChangingListener<T>.OnResetting(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (OnResetting == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnResetting?.Invoke(collection, items);
        }

        void IObservableCollectionChangingListener<T>.OnClearing(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnClearing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnClearing?.Invoke(collection);
        }

        #endregion
    }
}