using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestConditionCollectionComponent<TItem> : IConditionCollectionComponent, IHasPriority
    {
        #region Fields

        private readonly object _collection;

        #endregion

        #region Constructors

        public TestConditionCollectionComponent(IObservableCollection<TItem> collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public Func<IObservableCollection<TItem>, TItem, int, bool>? CanAdd { get; set; }

        public Func<IObservableCollection<TItem>, TItem, TItem, int, bool>? CanReplace { get; set; }

        public Func<IObservableCollection<TItem>, TItem, int, int, bool>? CanMove { get; set; }

        public Func<IObservableCollection<TItem>, TItem, int, bool>? CanRemove { get; set; }

        public Func<IObservableCollection<TItem>, IEnumerable<TItem>, bool>? CanReset { get; set; }

        public Func<IObservableCollection<TItem>, bool>? CanClear { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IConditionCollectionComponent.CanAdd<T>(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanAdd == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanAdd?.Invoke((IObservableCollection<TItem>)collection, (TItem)(object)item!, index) ?? true;
        }

        bool IConditionCollectionComponent.CanReplace<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanReplace == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReplace?.Invoke((IObservableCollection<TItem>)collection, (TItem)(object)oldItem!, (TItem)(object)newItem!, index) ?? true;
        }

        bool IConditionCollectionComponent.CanMove<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            _collection.ShouldEqual(collection);
            if (CanMove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanMove?.Invoke((IObservableCollection<TItem>)collection, (TItem)(object)item!, oldIndex, newIndex) ?? true;
        }

        bool IConditionCollectionComponent.CanRemove<T>(IObservableCollection<T> collection, T item, int index)
        {
            _collection.ShouldEqual(collection);
            if (CanRemove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanRemove?.Invoke((IObservableCollection<TItem>)collection, (TItem)(object)item!, index) ?? true;
        }

        bool IConditionCollectionComponent.CanReset<T>(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            _collection.ShouldEqual(collection);
            if (CanReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReset?.Invoke((IObservableCollection<TItem>)collection, items as IEnumerable<TItem> ?? items.Cast<TItem>()) ?? true;
        }

        bool IConditionCollectionComponent.CanClear<T>(IObservableCollection<T> collection)
        {
            _collection.ShouldEqual(collection);
            if (CanClear == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanClear?.Invoke((IObservableCollection<TItem>)collection) ?? true;
        }

        #endregion
    }
}