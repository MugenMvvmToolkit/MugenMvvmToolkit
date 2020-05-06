using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestObservableCollectionDecorator<T> : IObservableCollectionDecorator<T>, IHasPriority
    {
        #region Properties

        public Func<IEnumerable<T>, IEnumerable<T>>? DecorateItems { get; set; }

        public FuncRef<T, int, bool>? OnAdded { get; set; }

        public FuncRef<T, T, int, bool>? OnReplaced { get; set; }

        public FuncRef<T, int, int, bool>? OnMoved { get; set; }

        public FuncRef<T, int, bool>? OnRemoved { get; set; }

        public FuncRef<IEnumerable<T>, bool>? OnReset { get; set; }

        public FuncRef<T, int, object?, bool>? OnItemChanged { get; set; }

        public Func<bool>? OnCleared { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IEnumerable<T> IObservableCollectionDecorator<T>.DecorateItems(IEnumerable<T> items)
        {
            if (DecorateItems == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return DecorateItems?.Invoke(items) ?? items;
        }

        bool IObservableCollectionDecorator<T>.OnItemChanged(ref T item, ref int index, ref object? args)
        {
            if (OnItemChanged == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnItemChanged?.Invoke(ref item, ref index, ref args) ?? true;
        }

        bool IObservableCollectionDecorator<T>.OnAdded(ref T item, ref int index)
        {
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnAdded?.Invoke(ref item, ref index) ?? true;
        }

        bool IObservableCollectionDecorator<T>.OnReplaced(ref T oldItem, ref T newItem, ref int index)
        {
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReplaced?.Invoke(ref oldItem, ref newItem, ref index) ?? true;
        }

        bool IObservableCollectionDecorator<T>.OnMoved(ref T item, ref int oldIndex, ref int newIndex)
        {
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnMoved?.Invoke(ref item, ref oldIndex, ref newIndex) ?? true;
        }

        bool IObservableCollectionDecorator<T>.OnRemoved(ref T item, ref int index)
        {
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnRemoved?.Invoke(ref item, ref index) ?? true;
        }

        bool IObservableCollectionDecorator<T>.OnReset(ref IEnumerable<T> items)
        {
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReset?.Invoke(ref items) ?? true;
        }

        bool IObservableCollectionDecorator<T>.OnCleared()
        {
            if (OnCleared == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnCleared?.Invoke() ?? true;
        }

        #endregion
    }
}