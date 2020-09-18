using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class TestConditionCollectionComponent<T> : IConditionCollectionComponent<T>, IHasPriority
    {
        #region Fields

        private readonly object _collection;

        #endregion

        #region Constructors

        public TestConditionCollectionComponent(IObservableCollection<T> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Func<T, int, bool>? CanAdd { get; set; }

        public Func<T, T, int, bool>? CanReplace { get; set; }

        public Func<T, int, int, bool>? CanMove { get; set; }

        public Func<T, int, bool>? CanRemove { get; set; }

        public Func<IEnumerable<T>?, bool>? CanReset { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionCollectionComponent<T>.CanAdd(IReadOnlyCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanAdd == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanAdd?.Invoke(item!, index) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanReplace(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanReplace == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReplace?.Invoke(oldItem!, newItem!, index) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanMove(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (CanMove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanMove?.Invoke(item!, oldIndex, newIndex) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanRemove(IReadOnlyCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanRemove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanRemove?.Invoke(item!, index) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            _collection.ShouldEqual(collection);
            if (CanReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReset?.Invoke(items) ?? true;
        }

        #endregion
    }
}