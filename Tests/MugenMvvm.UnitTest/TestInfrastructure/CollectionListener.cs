using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class CollectionListener<T> : IObservableCollectionChangingListener<T>
    {
        #region Fields

        private readonly IObservableCollection<T> _collection;

        #endregion

        #region Constructors

        public CollectionListener(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Func<IObservableCollection<T>, T, int, bool>? OnAdding { get; set; }

        public Func<IObservableCollection<T>, T, T, int, bool>? OnReplacing { get; set; }

        public Func<IObservableCollection<T>, T, int, int, bool>? OnMoving { get; set; }

        public Func<IObservableCollection<T>, T, int, bool>? OnRemoving { get; set; }

        public Func<IObservableCollection<T>, IEnumerable<T>, bool>? OnResetting { get; set; }

        public Func<IObservableCollection<T>, bool>? OnClearing { get; set; }

        public Action<IObservableCollection<T>>? OnBeginBatchUpdate { get; set; }

        public Action<IObservableCollection<T>>? OnEndBatchUpdate { get; set; }

        public Action<IObservableCollection<T>, T, int>? OnAdded { get; set; }

        public Action<IObservableCollection<T>, T, T, int>? OnReplaced { get; set; }

        public Action<IObservableCollection<T>, T, int, int>? OnMoved { get; set; }

        public Action<IObservableCollection<T>, T, int>? OnRemoved { get; set; }

        public Action<IObservableCollection<T>, T, int, object?>? OnItemChanged { get; set; }

        public Action<IObservableCollection<T>, IEnumerable<T>>? OnReset { get; set; }

        public Action<IObservableCollection<T>>? OnCleared { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        #endregion

        #region Implementation of interfaces

        int IListener.GetPriority(object source)
        {
            return 0;
        }

        void IObservableCollectionChangedListener<T>.OnBeginBatchUpdate(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnBeginBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnBeginBatchUpdate?.Invoke(collection);
        }

        void IObservableCollectionChangedListener<T>.OnEndBatchUpdate(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnEndBatchUpdate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnEndBatchUpdate?.Invoke(collection);
        }

        void IObservableCollectionChangedListener<T>.OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args)
        {
            OnItemChanged?.Invoke(collection, item, index, args);
        }

        void IObservableCollectionChangedListener<T>.OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke(collection, item, index);
        }

        void IObservableCollectionChangedListener<T>.OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke(collection, oldItem, newItem, index);
        }

        void IObservableCollectionChangedListener<T>.OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke(collection, item, oldIndex, newIndex);
        }

        void IObservableCollectionChangedListener<T>.OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke(collection, item, index);
        }

        void IObservableCollectionChangedListener<T>.OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(collection, items);
        }

        void IObservableCollectionChangedListener<T>.OnCleared(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnCleared == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnCleared?.Invoke(collection);
        }

        bool IObservableCollectionChangingListener<T>.OnAdding(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdding == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnAdding?.Invoke(collection, item, index) ?? true;
        }

        bool IObservableCollectionChangingListener<T>.OnReplacing(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplacing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReplacing?.Invoke(collection, oldItem, newItem, index) ?? true;
        }

        bool IObservableCollectionChangingListener<T>.OnMoving(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnMoving?.Invoke(collection, item, oldIndex, newIndex) ?? true;
        }

        bool IObservableCollectionChangingListener<T>.OnRemoving(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnRemoving?.Invoke(collection, item, index) ?? true;
        }

        bool IObservableCollectionChangingListener<T>.OnResetting(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (OnResetting == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnResetting?.Invoke(collection, items) ?? true;
        }

        bool IObservableCollectionChangingListener<T>.OnClearing(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnClearing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnClearing?.Invoke(collection) ?? true;
        }

        #endregion
    }
}