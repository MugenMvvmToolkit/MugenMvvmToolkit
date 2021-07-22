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

        public Func<IEnumerable<object?>, object, int?>? TryGetIndex { get; set; }

        public FuncRef<object?, int, bool>? OnAdded { get; set; }

        public FuncRef<object?, object?, int, bool>? OnReplaced { get; set; }

        public FuncRef<object?, int, int, bool>? OnMoved { get; set; }

        public FuncRef<object?, int, bool>? OnRemoved { get; set; }

        public FuncRef<IEnumerable<object?>?, bool>? OnReset { get; set; }

        public FuncRef<object?, int, object?, bool>? OnChanged { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public bool HasAdditionalItems { get; set; } = true;

        public int Priority { get; set; }

        private void ThrowIfNeed(Delegate? handler)
        {
            if (handler == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
        }

        bool ICollectionDecorator.TryGetIndex(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object item, out int index)
        {
            ThrowIfNeed(TryGetIndex);
            var i = TryGetIndex?.Invoke(items, item);
            index = i.GetValueOrDefault(-1);
            return i.HasValue;
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items)
        {
            ThrowIfNeed(Decorate);
            return Decorate?.Invoke(items) ?? items;
        }

        bool ICollectionDecorator.OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args)
        {
            ThrowIfNeed(OnChanged);
            return OnChanged?.Invoke(ref item, ref index, ref args) ?? true;
        }

        bool ICollectionDecorator.OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            ThrowIfNeed(OnAdded);
            return OnAdded?.Invoke(ref item, ref index) ?? true;
        }

        bool ICollectionDecorator.OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            ThrowIfNeed(OnReplaced);
            return OnReplaced?.Invoke(ref oldItem, ref newItem, ref index) ?? true;
        }

        bool ICollectionDecorator.OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            ThrowIfNeed(OnMoved);
            return OnMoved?.Invoke(ref item, ref oldIndex, ref newIndex) ?? true;
        }

        bool ICollectionDecorator.OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            ThrowIfNeed(OnRemoved);
            return OnRemoved?.Invoke(ref item, ref index) ?? true;
        }

        bool ICollectionDecorator.OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            ThrowIfNeed(OnReset);
            return OnReset?.Invoke(ref items) ?? true;
        }
    }
}