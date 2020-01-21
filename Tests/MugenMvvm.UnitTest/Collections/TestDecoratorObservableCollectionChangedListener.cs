using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections
{
    public class TestDecoratorObservableCollectionChangedListener<T> : IDecoratorObservableCollectionChangedListener<T>, IHasPriority
    {
        #region Fields

        private readonly IObservableCollection<T> _collection;

        #endregion

        #region Constructors

        public TestDecoratorObservableCollectionChangedListener(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<IObservableCollection<T>, T, int>? OnAdded { get; set; }

        public Action<IObservableCollection<T>, T, T, int>? OnReplaced { get; set; }

        public Action<IObservableCollection<T>, T, int, int>? OnMoved { get; set; }

        public Action<IObservableCollection<T>, T, int>? OnRemoved { get; set; }

        public Action<IObservableCollection<T>, T, int, object?>? OnItemChanged { get; set; }

        public Action<IObservableCollection<T>, IEnumerable<T>>? OnReset { get; set; }

        public Action<IObservableCollection<T>>? OnCleared { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDecoratorObservableCollectionChangedListener<T>.OnItemChanged(IObservableCollection<T> collection, T item, int index, object? args)
        {
            OnItemChanged?.Invoke(collection, item, index, args);
        }

        void IDecoratorObservableCollectionChangedListener<T>.OnAdded(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke(collection, item, index);
        }

        void IDecoratorObservableCollectionChangedListener<T>.OnReplaced(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke(collection, oldItem, newItem, index);
        }

        void IDecoratorObservableCollectionChangedListener<T>.OnMoved(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke(collection, item, oldIndex, newIndex);
        }

        void IDecoratorObservableCollectionChangedListener<T>.OnRemoved(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke(collection, item, index);
        }

        void IDecoratorObservableCollectionChangedListener<T>.OnReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(collection, items);
        }

        void IDecoratorObservableCollectionChangedListener<T>.OnCleared(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnCleared == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnCleared?.Invoke(collection);
        }

        #endregion
    }
}