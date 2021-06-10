﻿using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Collections
{
    public class TestCollectionChangedListener<T> : ICollectionChangedListener<T>, IHasPriority
    {
        public Action<IReadOnlyCollection<T>, T, int>? OnAdded { get; set; }

        public Action<IReadOnlyCollection<T>, T, T, int>? OnReplaced { get; set; }

        public Action<IReadOnlyCollection<T>, T, int, int>? OnMoved { get; set; }

        public Action<IReadOnlyCollection<T>, T, int>? OnRemoved { get; set; }

        public Action<IReadOnlyCollection<T>, T, int, object?>? OnChanged { get; set; }

        public Action<IReadOnlyCollection<T>, IEnumerable<T>?>? OnReset { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        void ICollectionChangedListener<T>.OnChanged(IReadOnlyCollection<T> collection, T item, int index, object? args) => OnChanged?.Invoke(collection, item!, index, args);

        void ICollectionChangedListener<T>.OnAdded(IReadOnlyCollection<T> collection, T item, int index)
        {
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke(collection, item!, index);
        }

        void ICollectionChangedListener<T>.OnReplaced(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke(collection, oldItem!, newItem!, index);
        }

        void ICollectionChangedListener<T>.OnMoved(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke(collection, item!, oldIndex, newIndex);
        }

        void ICollectionChangedListener<T>.OnRemoved(IReadOnlyCollection<T> collection, T item, int index)
        {
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke(collection, item!, index);
        }

        void ICollectionChangedListener<T>.OnReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(collection, items);
        }
    }
}