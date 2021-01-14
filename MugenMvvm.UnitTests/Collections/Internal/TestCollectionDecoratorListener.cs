using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using Should;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class TestCollectionDecoratorListener<T> : ICollectionDecoratorListener
    {
        private readonly object _collection;

        public TestCollectionDecoratorListener(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public Action<T, int>? OnAdded { get; set; }

        public Action<T, T, int>? OnReplaced { get; set; }

        public Action<T, int, int>? OnMoved { get; set; }

        public Action<T, int>? OnRemoved { get; set; }

        public Action<T, int, object?>? OnItemChanged { get; set; }

        public Action<IEnumerable<T>?>? OnReset { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        void ICollectionDecoratorListener.OnItemChanged(ICollection collection, object? item, int index, object? args)
        {
            _collection.ShouldEqual(collection);
            OnItemChanged?.Invoke((T) item!, index, args);
        }

        void ICollectionDecoratorListener.OnAdded(ICollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke((T) item!, index);
        }

        void ICollectionDecoratorListener.OnReplaced(ICollection collection, object? oldItem, object? newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke((T) oldItem!, (T) newItem!, index);
        }

        void ICollectionDecoratorListener.OnMoved(ICollection collection, object? item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke((T) item!, oldIndex, newIndex);
        }

        void ICollectionDecoratorListener.OnRemoved(ICollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke((T) item!, index);
        }

        void ICollectionDecoratorListener.OnReset(ICollection collection, IEnumerable<object?>? items)
        {
            _collection.ShouldEqual(collection);
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(items as IEnumerable<T> ?? items?.Cast<T>());
        }
    }
}