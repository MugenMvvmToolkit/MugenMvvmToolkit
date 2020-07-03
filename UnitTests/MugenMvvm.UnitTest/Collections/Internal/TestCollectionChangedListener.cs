using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionChangedListener<TITem> : ICollectionChangedListener, IHasPriority
    {
        #region Fields

        private readonly object _collection;

        #endregion

        #region Constructors

        public TestCollectionChangedListener(IObservableCollection<TITem> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<IObservableCollection<TITem>, TITem, int>? OnAdded { get; set; }

        public Action<IObservableCollection<TITem>, TITem, TITem, int>? OnReplaced { get; set; }

        public Action<IObservableCollection<TITem>, TITem, int, int>? OnMoved { get; set; }

        public Action<IObservableCollection<TITem>, TITem, int>? OnRemoved { get; set; }

        public Action<IObservableCollection<TITem>, TITem, int, object?>? OnItemChanged { get; set; }

        public Action<IObservableCollection<TITem>, IEnumerable<TITem>>? OnReset { get; set; }

        public Action<IObservableCollection<TITem>>? OnCleared { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICollectionChangedListener.OnItemChanged<T>(IObservableCollection<T> collection, T item, int index, object? args)
        {
            OnItemChanged?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, index, args);
        }

        void ICollectionChangedListener.OnAdded<T>(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, index);
        }

        void ICollectionChangedListener.OnReplaced<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)oldItem!, (TITem)(object)newItem!, index);
        }

        void ICollectionChangedListener.OnMoved<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, oldIndex, newIndex);
        }

        void ICollectionChangedListener.OnRemoved<T>(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, index);
        }

        void ICollectionChangedListener.OnReset<T>(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke((IObservableCollection<TITem>)collection, items as IEnumerable<TITem> ?? items.Cast<TITem>());
        }

        void ICollectionChangedListener.OnCleared<T>(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnCleared == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnCleared?.Invoke((IObservableCollection<TITem>)collection);
        }

        #endregion
    }
}