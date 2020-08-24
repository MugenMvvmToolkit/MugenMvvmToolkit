using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public abstract class TestCollectionChangedListenerBase<T> : ICollectionChangedListenerBase, IHasPriority
    {
        #region Fields

        private readonly IObservableCollection _collection;

        #endregion

        #region Constructors

        protected TestCollectionChangedListenerBase(IObservableCollection<T> collection)
        {
            _collection = (IObservableCollection)collection;
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

        void ICollectionChangedListenerBase.OnItemChanged(IObservableCollection collection, object? item, int index, object? args)
        {
            _collection.ShouldEqual(collection);
            OnItemChanged?.Invoke((T)item!, index, args);
        }

        void ICollectionChangedListenerBase.OnAdded(IObservableCollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke((T)item!, index);
        }

        void ICollectionChangedListenerBase.OnReplaced(IObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke((T)oldItem!, (T)newItem!, index);
        }

        void ICollectionChangedListenerBase.OnMoved(IObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke((T)item!, oldIndex, newIndex);
        }

        void ICollectionChangedListenerBase.OnRemoved(IObservableCollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke((T)item!, index);
        }

        void ICollectionChangedListenerBase.OnReset(IObservableCollection collection, IEnumerable<object?>? items)
        {
            _collection.ShouldEqual(collection);
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(items as IEnumerable<T> ?? items?.Cast<T>());
        }

        #endregion
    }
}