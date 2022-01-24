using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(ReadOnlyObservableCollectionDebuggerProxy<>))]
    public sealed class ObservableSet<T> : SynchronizableComponentOwnerBase<IReadOnlyObservableCollection>, IObservableCollection<T>, IObservableCollection,
        IHasComponentAddConditionHandler, IHasFindAllIndexOfSupport where T : notnull
    {
        private readonly Dictionary<T, LinkedListNode<T>> _dictionary;
        private readonly LinkedList<T> _list;

        public ObservableSet(IEqualityComparer<T>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : this(null, comparer, componentCollectionManager)
        {
        }

        public ObservableSet(int capacity, IEqualityComparer<T>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _dictionary = new Dictionary<T, LinkedListNode<T>>(capacity, comparer);
            _list = new LinkedList<T>();
        }

        public ObservableSet(IEnumerable<T>? items, IEqualityComparer<T>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            _list = new LinkedList<T>();
            if (items != null)
                ResetInternal(items);
        }

        public IEqualityComparer<T> Comparer => _dictionary.Comparer;

        public Optional<T> First
        {
            get
            {
                using var _ = Lock();
                return _list.First == null ? default(Optional<T>) : _list.First.Value;
            }
        }

        public Optional<T> Last
        {
            get
            {
                using var _ = Lock();
                return _list.Last == null ? default(Optional<T>) : _list.Last.Value;
            }
        }

        public bool IsReadOnly => false;

        public bool IsDisposed { get; private set; }

        public int Count => _list.Count;

        public bool IsSet => true;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot
        {
            get
            {
                ExceptionManager.ThrowNotSupported(nameof(ICollection.SyncRoot));
                return null;
            }
        }

        Type IReadOnlyObservableCollection.ItemType => typeof(T);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            EnsureNotDisposed();
            return new Enumerator(this);
        }

        public bool Add(T item)
        {
            using (Lock())
            {
                EnsureNotDisposed();
                if (_dictionary.ContainsKey(item))
                    return false;

                var index = _list.Count;
                if (!GetComponents<IConditionCollectionComponent<T>>().CanAdd(this, item, index))
                    return false;

                GetComponents<ICollectionChangingListener<T>>().OnAdding(this, item, index);
                _dictionary[item] = _list.AddLast(item);
                GetComponents<ICollectionChangedListener<T>>().OnAdded(this, item, index);
                return true;
            }
        }

        public bool AddOrUpdate(T item)
        {
            using (Lock())
            {
                EnsureNotDisposed();
                if (_dictionary.TryGetValue(item, out var node))
                {
                    var oldItem = node.Value;
                    if (InternalEqualityComparer.GetReferenceComparer<T>().Equals(item, oldItem))
                        return true;

                    int? index = null;
                    if (!GetComponentsNullable<IConditionCollectionComponent<T>>()?.CanReplace(this, oldItem, item, index ??= IndexOf(node)) ?? false)
                        return false;

                    GetComponentsNullable<ICollectionChangingListener<T>>()?.OnReplacing(this, oldItem, item, index ??= IndexOf(node));
                    node.Value = item;
                    _dictionary.Remove(item);
                    _dictionary[item] = node;
                    GetComponentsNullable<ICollectionChangedListener<T>>()?.OnReplaced(this, oldItem, item, index ?? IndexOf(node));
                }
                else
                {
                    var index = _list.Count;
                    if (!GetComponents<IConditionCollectionComponent<T>>().CanAdd(this, item, index))
                        return false;

                    GetComponents<ICollectionChangingListener<T>>().OnAdding(this, item, index);
                    _dictionary[item] = _list.AddLast(item);
                    GetComponents<ICollectionChangedListener<T>>().OnAdded(this, item, index);
                }

                return true;
            }
        }

        public void Clear() => Reset(null);

        public bool Remove(T item)
        {
            using (Lock())
            {
                EnsureNotDisposed();
                if (!_dictionary.TryGetValue(item, out var node))
                    return false;

                int? index = null;
                if (!GetComponentsNullable<IConditionCollectionComponent<T>>()?.CanRemove(this, node.Value, index ??= IndexOf(node)) ?? false)
                    return false;

                GetComponentsNullable<ICollectionChangingListener<T>>()?.OnRemoving(this, node.Value, index ??= IndexOf(node));
                _dictionary.Remove(item);
                _list.Remove(node);
                GetComponentsNullable<ICollectionChangedListener<T>>()?.OnRemoved(this, node.Value, index ?? IndexOf(node));

                return true;
            }
        }

        public bool Contains(T? item)
        {
            if (item == null)
                return false;
            using (Lock())
            {
                return _dictionary.ContainsKey(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Should.NotBeNull(array, nameof(array));
            using (Lock())
            {
                EnsureNotDisposed();
                _list.CopyTo(array, arrayIndex);
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            using (Lock())
            {
                if (IsDisposed)
                    return;
                var components = GetComponents<IDisposableComponent<IReadOnlyObservableCollection>>();
                components.OnDisposing(this, null);
                components.OnDisposed(this, null);
                this.ClearComponents();
                IsDisposed = true;
            }
        }

        public bool Reset(IEnumerable<T>? items)
        {
            if (ReferenceEquals(items, this))
                return false;

            using (Lock())
            {
                EnsureNotDisposed();
                if (items == null && _list.Count == 0 || !GetComponents<IConditionCollectionComponent<T>>().CanReset(this, items))
                    return false;

                GetComponents<ICollectionChangingListener<T>>().OnResetting(this, items);
                _dictionary.Clear();
                _list.Clear();
                if (items != null)
                    items = ResetInternal(items);

                GetComponents<ICollectionChangedListener<T>>().OnReset(this, _list.Count == 0 ? null : items);
                return true;
            }
        }

        private IEnumerable<T> ResetInternal(IEnumerable<T> items)
        {
            var hasDup = false;
            foreach (var item in items)
            {
                if (_dictionary.ContainsKey(item))
                    hasDup = true;
                else
                    _dictionary[item] = _list.AddLast(item);
            }

            return hasDup ? _list : items;
        }

        private EnumeratorRef GetEnumeratorRef()
        {
            EnsureNotDisposed();
            return new EnumeratorRef(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (IsDisposed)
                ExceptionManager.ThrowObjectDisposed(this);
        }

        private int IndexOf(LinkedListNode<T> node)
        {
            if (node.Next == null)
                return _list.Count - 1;
            if (node.Previous == null)
                return 0;

            var count = 0;
            var prevNode = node.Previous;
            var nextNode = node.Next;
            while (prevNode != null && nextNode != null)
            {
                prevNode = prevNode?.Previous;
                nextNode = nextNode?.Next;
                ++count;
            }

            if (prevNode == null)
                return count;
            return _list.Count - count - 1;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            using (Lock())
            {
                EnsureNotDisposed();
                ((ICollection) _list).CopyTo(array, index);
            }
        }

        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorRef();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorRef();

        bool IHasComponentAddConditionHandler.CanAddComponent(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        void IHasFindAllIndexOfSupport.FindAllIndexOf(object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (item is not T value)
                return;

            if (_dictionary.TryGetValue(value, out var node))
                indexes.Add(IndexOf(node));
        }

        bool IObservableCollection.Reset(IEnumerable? items) => Reset(items as IEnumerable<T> ?? items?.Cast<T>());

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private LinkedList<T>.Enumerator _enumerator;
            private ActionToken _locker;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ObservableSet<T> collection)
            {
                _enumerator = collection._list.GetEnumerator();
                _locker = collection.Lock();
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _enumerator.Current!;
            }

            object IEnumerator.Current => _enumerator.Current!;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _enumerator.MoveNext();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => ((IEnumerator) _enumerator).Reset();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _locker.Dispose();
        }

        private sealed class EnumeratorRef : IEnumerator<T>
        {
            private LinkedList<T>.Enumerator _enumerator;
            private ActionToken _locker;

            internal EnumeratorRef(ObservableSet<T> collection)
            {
                _enumerator = collection._list.GetEnumerator();
                _locker = collection.Lock();
            }

            public T Current => _enumerator.Current!;

            object IEnumerator.Current => _enumerator.Current!;

            public void Dispose() => _locker.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => ((IEnumerator) _enumerator).Reset();
        }
    }
}