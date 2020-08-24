using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionChangedListenerGeneric<T> : ICollectionChangedListener<T>, IHasPriority
    {
        #region Fields

        private readonly object _collection;

        #endregion

        #region Constructors

        public TestCollectionChangedListenerGeneric(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<T, int>? OnAdded { get; set; }

        public Action<T, T, int>? OnReplaced { get; set; }

        public Action<T, int, int>? OnMoved { get; set; }

        public Action<T, int>? OnRemoved { get; set; }

        public Action<T, int, object?>? OnItemChanged { get; set; }

        public Action<IEnumerable<T>?>? OnReset { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICollectionChangedListener<T>.OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args)
        {
            _collection.ShouldEqual(collection);
            OnItemChanged?.Invoke(item!, index, args);
        }

        void ICollectionChangedListener<T>.OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke(item!, index);
        }

        void ICollectionChangedListener<T>.OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke(oldItem!, newItem!, index);
        }

        void ICollectionChangedListener<T>.OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke(item!, oldIndex, newIndex);
        }

        void ICollectionChangedListener<T>.OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke(item!, index);
        }

        void ICollectionChangedListener<T>.OnReset(IObservableCollection<T> collection, IEnumerable<T>? items)
        {
            _collection.ShouldEqual(collection);
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(items);
        }

        #endregion
    }
}