using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionChangingListener<T> : ICollectionChangingListener, IHasPriority
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

        public Action<IEnumerable<T>>? OnResetting { get; set; }

        public Action? OnClearing { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICollectionChangingListener.OnAdding(IObservableCollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdding == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdding?.Invoke((T) item!, index);
        }

        void ICollectionChangingListener.OnReplacing(IObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplacing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplacing?.Invoke((T) oldItem!, (T) newItem!, index);
        }

        void ICollectionChangingListener.OnMoving(IObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoving?.Invoke((T) item!, oldIndex, newIndex);
        }

        void ICollectionChangingListener.OnRemoving(IObservableCollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoving?.Invoke((T) item!, index);
        }

        void ICollectionChangingListener.OnResetting(IObservableCollection collection, IEnumerable<object?> items)
        {
            _collection.ShouldEqual(collection);
            if (OnResetting == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnResetting?.Invoke(items.Cast<T>());
        }

        void ICollectionChangingListener.OnClearing(IObservableCollection collection)
        {
            _collection.ShouldEqual(collection);
            if (OnClearing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnClearing?.Invoke();
        }

        #endregion
    }
}