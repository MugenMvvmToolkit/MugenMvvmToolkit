using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Collections
{
    public class TestCollectionDecorator : ICollectionDecorator, IHasPriority
    {
        public Func<IEnumerable<object?>, IEnumerable<object?>>? Decorate { get; set; }

        public FuncRef<object?, int, bool>? OnAdded { get; set; }

        public FuncRef<object?, object?, int, bool>? OnReplaced { get; set; }

        public FuncRef<object?, int, int, bool>? OnMoved { get; set; }

        public FuncRef<object?, int, bool>? OnRemoved { get; set; }

        public FuncRef<IEnumerable<object?>?, bool>? OnReset { get; set; }

        public FuncRef<object?, int, object?, bool>? OnChanged { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        IEnumerable<object?> ICollectionDecorator.Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items)
        {
            if (Decorate == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return Decorate?.Invoke(items) ?? items;
        }

        bool ICollectionDecorator.OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args)
        {
            if (OnChanged == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnChanged?.Invoke(ref item, ref index, ref args) ?? true;
        }

        bool ICollectionDecorator.OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnAdded?.Invoke(ref item, ref index) ?? true;
        }

        bool ICollectionDecorator.OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReplaced?.Invoke(ref oldItem, ref newItem, ref index) ?? true;
        }

        bool ICollectionDecorator.OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnMoved?.Invoke(ref item, ref oldIndex, ref newIndex) ?? true;
        }

        bool ICollectionDecorator.OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnRemoved?.Invoke(ref item, ref index) ?? true;
        }

        bool ICollectionDecorator.OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return OnReset?.Invoke(ref items) ?? true;
        }
    }
}