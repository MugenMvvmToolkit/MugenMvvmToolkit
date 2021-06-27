using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Collections
{
    public class TestCollectionChangingListener<T> : ICollectionChangingListener<T>, IHasPriority
    {
        public Action<IReadOnlyObservableCollection<T>, T, int>? OnAdding { get; set; }

        public Action<IReadOnlyObservableCollection<T>, T, T, int>? OnReplacing { get; set; }

        public Action<IReadOnlyObservableCollection<T>, T, int, int>? OnMoving { get; set; }

        public Action<IReadOnlyObservableCollection<T>, T, int>? OnRemoving { get; set; }

        public Action<IReadOnlyObservableCollection<T>, IReadOnlyCollection<T>?>? OnResetting { get; set; }

        public bool ThrowErrorNullDelegate { get; set; }

        public int Priority { get; set; }

        void ICollectionChangingListener<T>.OnAdding(IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            if (OnAdding == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnAdding?.Invoke(collection, item!, index);
        }

        void ICollectionChangingListener<T>.OnReplacing(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            if (OnReplacing == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnReplacing?.Invoke(collection, oldItem!, newItem!, index);
        }

        void ICollectionChangingListener<T>.OnMoving(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            if (OnMoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnMoving?.Invoke(collection, item!, oldIndex, newIndex);
        }

        void ICollectionChangingListener<T>.OnRemoving(IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            if (OnRemoving == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnRemoving?.Invoke(collection, item!, index);
        }

        void ICollectionChangingListener<T>.OnResetting(IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items)
        {
            if (OnResetting == null && ThrowErrorNullDelegate)
                throw new NotSupportedException();
            OnResetting?.Invoke(collection, items);
        }
    }
}