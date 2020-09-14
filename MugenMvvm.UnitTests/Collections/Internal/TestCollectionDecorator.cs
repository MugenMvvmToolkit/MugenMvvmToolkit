using System;
using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class TestCollectionDecorator : ICollectionDecorator, IHasPriority
    {
        #region Properties

        public Func<IEnumerable<object?>, IEnumerable<object?>>? DecorateItems { get; set; }

        public FuncRef<object?, int, bool>? OnAdded { get; set; }

        public FuncRef<object?, object?, int, bool>? OnReplaced { get; set; }

        public FuncRef<object?, int, int, bool>? OnMoved { get; set; }

        public FuncRef<object?, int, bool>? OnRemoved { get; set; }

        public FuncRef<IEnumerable<object?>?, bool>? OnReset { get; set; }

        public FuncRef<object?, int, object?, bool>? OnItemChanged { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IEnumerable<object?> ICollectionDecorator.DecorateItems(ICollection collection, IEnumerable<object?> items)
        {
            if (DecorateItems == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return DecorateItems?.Invoke(items) ?? items;
        }

        bool ICollectionDecorator.OnItemChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (OnItemChanged == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnItemChanged?.Invoke(ref item, ref index, ref args) ?? true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnAdded?.Invoke(ref item, ref index) ?? true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReplaced?.Invoke(ref oldItem, ref newItem, ref index) ?? true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnMoved?.Invoke(ref item, ref oldIndex, ref newIndex) ?? true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnRemoved?.Invoke(ref item, ref index) ?? true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReset?.Invoke(ref items) ?? true;
        }

        #endregion
    }
}