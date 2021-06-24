﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Tests.Collections
{
    public class TestCollectionDecoratorListener<T> : ICollectionDecoratorListener
    {
        public Action<IReadOnlyObservableCollection, T, int>? OnAdded { get; set; }

        public Action<IReadOnlyObservableCollection, T, T, int>? OnReplaced { get; set; }

        public Action<IReadOnlyObservableCollection, T, int, int>? OnMoved { get; set; }

        public Action<IReadOnlyObservableCollection, T, int>? OnRemoved { get; set; }

        public Action<IReadOnlyObservableCollection, T, int, object?>? OnChanged { get; set; }

        public Action<IReadOnlyObservableCollection, IEnumerable<T>?>? OnReset { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        void ICollectionDecoratorListener.OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args) =>
            OnChanged?.Invoke(collection, (T)item!, index, args);

        void ICollectionDecoratorListener.OnAdded(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (OnAdded == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdded?.Invoke(collection, (T)item!, index);
        }

        void ICollectionDecoratorListener.OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            if (OnReplaced == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplaced?.Invoke(collection, (T)oldItem!, (T)newItem!, index);
        }

        void ICollectionDecoratorListener.OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            if (OnMoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoved?.Invoke(collection, (T)item!, oldIndex, newIndex);
        }

        void ICollectionDecoratorListener.OnRemoved(IReadOnlyObservableCollection collection, object? item, int index)
        {
            if (OnRemoved == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoved?.Invoke(collection, (T)item!, index);
        }

        void ICollectionDecoratorListener.OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items)
        {
            if (OnReset == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReset?.Invoke(collection, items as IEnumerable<T> ?? items?.Cast<T>());
        }
    }
}