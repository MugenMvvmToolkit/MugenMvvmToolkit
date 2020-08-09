using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestConditionCollectionComponent<T> : IConditionCollectionComponent, IHasPriority
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

        public Func<IEnumerable<T>, bool>? CanReset { get; set; }

        public Func<bool>? CanClear { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionCollectionComponent.CanAdd(IObservableCollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanAdd == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanAdd?.Invoke((T) item!, index) ?? true;
        }

        bool IConditionCollectionComponent.CanReplace(IObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanReplace == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReplace?.Invoke((T) oldItem!, (T) newItem!, index) ?? true;
        }

        bool IConditionCollectionComponent.CanMove(IObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (CanMove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanMove?.Invoke((T) item!, oldIndex, newIndex) ?? true;
        }

        bool IConditionCollectionComponent.CanRemove(IObservableCollection collection, object? item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanRemove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanRemove?.Invoke((T) item!, index) ?? true;
        }

        bool IConditionCollectionComponent.CanReset(IObservableCollection collection, IEnumerable<object?> items)
        {
            _collection.ShouldEqual(collection);
            if (CanReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReset?.Invoke(items.Cast<T>()) ?? true;
        }

        bool IConditionCollectionComponent.CanClear(IObservableCollection collection)
        {
            _collection.ShouldEqual(collection);
            if (CanClear == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanClear?.Invoke() ?? true;
        }

        #endregion
    }
}