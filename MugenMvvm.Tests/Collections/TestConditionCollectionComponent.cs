using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Collections
{
    public class TestConditionCollectionComponent<T> : IConditionCollectionComponent<T>, IHasPriority
    {
        public Func<IReadOnlyCollection<T>, T, int, bool>? CanAdd { get; set; }

        public Func<IReadOnlyCollection<T>, T, T, int, bool>? CanReplace { get; set; }

        public Func<IReadOnlyCollection<T>, T, int, int, bool>? CanMove { get; set; }

        public Func<IReadOnlyCollection<T>, T, int, bool>? CanRemove { get; set; }

        public Func<IReadOnlyCollection<T>, IEnumerable<T>?, bool>? CanReset { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        bool IConditionCollectionComponent<T>.CanAdd(IReadOnlyCollection<T> collection, T item, int index)
        {
            if (CanAdd == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanAdd?.Invoke(collection, item!, index) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanReplace(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            if (CanReplace == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReplace?.Invoke(collection, oldItem!, newItem!, index) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanMove(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            if (CanMove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanMove?.Invoke(collection, item!, oldIndex, newIndex) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanRemove(IReadOnlyCollection<T> collection, T item, int index)
        {
            if (CanRemove == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanRemove?.Invoke(collection, item!, index) ?? true;
        }

        bool IConditionCollectionComponent<T>.CanReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            if (CanReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            return CanReset?.Invoke(collection, items) ?? true;
        }
    }
}