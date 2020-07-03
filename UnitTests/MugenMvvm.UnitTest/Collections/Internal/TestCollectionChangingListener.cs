using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionChangingListener<TITem> : ICollectionChangingListener, IHasPriority
    {
        #region Fields

        private readonly object _collection;

        #endregion

        #region Constructors

        public TestCollectionChangingListener(IObservableCollection<TITem> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Action<IObservableCollection<TITem>, TITem, int>? OnAdding { get; set; }

        public Action<IObservableCollection<TITem>, TITem, TITem, int>? OnReplacing { get; set; }

        public Action<IObservableCollection<TITem>, TITem, int, int>? OnMoving { get; set; }

        public Action<IObservableCollection<TITem>, TITem, int>? OnRemoving { get; set; }

        public Action<IObservableCollection<TITem>, IEnumerable<TITem>>? OnResetting { get; set; }

        public Action<IObservableCollection<TITem>>? OnClearing { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void ICollectionChangingListener.OnAdding<T>(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnAdding == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdding?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, index);
        }

        void ICollectionChangingListener.OnReplacing<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnReplacing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplacing?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)oldItem!, (TITem)(object)newItem!, index);
        }

        void ICollectionChangingListener.OnMoving<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (OnMoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoving?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, oldIndex, newIndex);
        }

        void ICollectionChangingListener.OnRemoving<T>(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (OnRemoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoving?.Invoke((IObservableCollection<TITem>)collection, (TITem)(object)item!, index);
        }

        void ICollectionChangingListener.OnResetting<T>(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (OnResetting == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnResetting?.Invoke((IObservableCollection<TITem>)collection, items as IEnumerable<TITem> ?? items.Cast<TITem>());
        }

        void ICollectionChangingListener.OnClearing<T>(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (OnClearing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnClearing?.Invoke((IObservableCollection<TITem>)collection);
        }

        #endregion
    }
}