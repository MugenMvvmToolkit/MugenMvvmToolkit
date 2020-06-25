using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestConditionObservableCollectionComponent<T> : IConditionObservableCollectionComponent<T>, IHasPriority
    {
        #region Fields

        private readonly IObservableCollection<T> _collection;

        #endregion

        #region Constructors

        public TestConditionObservableCollectionComponent(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Func<IObservableCollection<T>, T, int, bool>? CanAdd { get; set; }

        public Func<IObservableCollection<T>, T, T, int, bool>? CanReplace { get; set; }

        public Func<IObservableCollection<T>, T, int, int, bool>? CanMove { get; set; }

        public Func<IObservableCollection<T>, T, int, bool>? CanRemove { get; set; }

        public Func<IObservableCollection<T>, IEnumerable<T>, bool>? CanReset { get; set; }

        public Func<IObservableCollection<T>, bool>? CanClear { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionObservableCollectionComponent<T>.CanAdd(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanAdd == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanAdd?.Invoke(collection, item, index) ?? true;
        }

        bool IConditionObservableCollectionComponent<T>.CanReplace(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanReplace == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReplace?.Invoke(collection, oldItem, newItem, index) ?? true;
        }

        bool IConditionObservableCollectionComponent<T>.CanMove(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (CanMove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanMove?.Invoke(collection, item, oldIndex, newIndex) ?? true;
        }

        bool IConditionObservableCollectionComponent<T>.CanRemove(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanRemove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanRemove?.Invoke(collection, item, index) ?? true;
        }

        bool IConditionObservableCollectionComponent<T>.CanReset(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (CanReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReset?.Invoke(collection, items) ?? true;
        }

        bool IConditionObservableCollectionComponent<T>.CanClear(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (CanClear == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanClear?.Invoke(collection) ?? true;
        }

        #endregion
    }
}