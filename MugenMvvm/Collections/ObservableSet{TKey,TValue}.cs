using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    [DebuggerDisplay("Count={" + nameof(Count) + "} " + "{" + nameof(Locker) + "}")]
    [DebuggerTypeProxy(typeof(ReadOnlyObservableCollectionDebuggerProxy<>))]
    public class ObservableSet<TKey, TValue> : SynchronizableComponentOwnerBase<IReadOnlyObservableCollection>, IObservableCollection<TValue>, IObservableCollection,
        IHasComponentAddConditionHandler, IHasFindAllIndexOfSupport where TKey : notnull
    {
        private readonly Func<TValue, TKey> _getKey;
        private readonly Dictionary<TKey, LinkedListNode<TValue>> _dictionary;
        private readonly LinkedList<TValue> _list;

        public ObservableSet(Func<TValue, TKey> getKey, IEqualityComparer<TKey>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : this(getKey, null, comparer, componentCollectionManager)
        {
        }

        public ObservableSet(Func<TValue, TKey> getKey, int capacity, IEqualityComparer<TKey>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            Should.NotBeNull(getKey, nameof(getKey));
            _getKey = getKey;
            _dictionary = new Dictionary<TKey, LinkedListNode<TValue>>(capacity, comparer);
            _list = new LinkedList<TValue>();
        }

        public ObservableSet(Func<TValue, TKey> getKey, IEnumerable<TValue>? items, IEqualityComparer<TKey>? comparer = null,
            IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            Should.NotBeNull(getKey, nameof(getKey));
            _getKey = getKey;
            _dictionary = new Dictionary<TKey, LinkedListNode<TValue>>(comparer);
            _list = new LinkedList<TValue>();
            if (items != null)
                ResetInternal(items);
        }

        public IEqualityComparer<TKey> Comparer => _dictionary.Comparer;

        public Optional<TValue> First
        {
            get
            {
                using var _ = Lock();
                return _list.First == null ? default(Optional<TValue>) : _list.First.Value;
            }
        }

        public Optional<TValue> Last
        {
            get
            {
                using var _ = Lock();
                return _list.Last == null ? default(Optional<TValue>) : _list.Last.Value;
            }
        }

        public Optional<TKey> NullValueKey { get; set; }

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

        Type IReadOnlyObservableCollection.ItemType => typeof(TValue);

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            using var _ = Lock();
            if (_dictionary.TryGetValue(key, out var node))
            {
                value = node.Value;
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TKey GetKey(TValue item) => _getKey(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        public bool Add(TValue item, bool addLast = true)
        {
            var key = _getKey(item);
            using var _ = Lock();
            if (_dictionary.ContainsKey(key))
                return false;

            var index = addLast ? _list.Count : 0;
            if (!GetComponents<IConditionCollectionComponent<TValue>>().CanAdd(this, item, index))
                return false;

            GetComponents<ICollectionChangingListener<TValue>>().OnAdding(this, item, index);
            _dictionary[key] = addLast ? _list.AddLast(item) : _list.AddFirst(item);
            GetComponents<ICollectionChangedListener<TValue>>().OnAdded(this, item, index);
            return true;
        }

        public bool AddOrUpdate(TValue item, bool addLast = true)
        {
            var key = _getKey(item);
            using var _ = Lock();
            if (_dictionary.TryGetValue(key, out var node))
            {
                var oldItem = node.Value;
                if (InternalEqualityComparer.GetReferenceComparer<TValue>().Equals(item, oldItem))
                    return false;

                int? index = null;
                if (!GetComponentsNullable<IConditionCollectionComponent<TValue>>()?.CanReplace(this, oldItem, item, index ??= IndexOf(node)) ?? false)
                    return false;

                GetComponentsNullable<ICollectionChangingListener<TValue>>()?.OnReplacing(this, oldItem, item, index ??= IndexOf(node));
                node.Value = item;
                _dictionary.Remove(key);
                _dictionary[key] = node;
                GetComponentsNullable<ICollectionChangedListener<TValue>>()?.OnReplaced(this, oldItem, item, index ?? IndexOf(node));
            }
            else
            {
                var index = addLast ? _list.Count : 0;
                if (!GetComponents<IConditionCollectionComponent<TValue>>().CanAdd(this, item, index))
                    return false;

                GetComponents<ICollectionChangingListener<TValue>>().OnAdding(this, item, index);
                _dictionary[key] = addLast ? _list.AddLast(item) : _list.AddFirst(item);
                GetComponents<ICollectionChangedListener<TValue>>().OnAdded(this, item, index);
            }

            return true;
        }

        public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            using var _ = Lock();
            if (!_dictionary.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            int? index = null;
            if (!GetComponentsNullable<IConditionCollectionComponent<TValue>>()?.CanRemove(this, node.Value, index ??= IndexOf(node)) ?? false)
            {
                value = default;
                return false;
            }

            value = node.Value;
            GetComponentsNullable<ICollectionChangingListener<TValue>>()?.OnRemoving(this, node.Value, index ??= IndexOf(node));
            _dictionary.Remove(key);
            _list.Remove(node);
            GetComponentsNullable<ICollectionChangedListener<TValue>>()?.OnRemoved(this, node.Value, index ?? IndexOf(node));
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            using var _ = Lock();
            return _dictionary.ContainsKey(key);
        }

        public void Clear() => Reset(null);

        public bool Remove(TValue item) => Remove(_getKey(item), out _);

        public bool Contains(TValue item) => ContainsKey(_getKey(item));

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            Should.NotBeNull(array, nameof(array));
            using var _ = Lock();
            _list.CopyTo(array, arrayIndex);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            using var _ = Lock(false);
            if (IsDisposed)
                return;
            var components = GetComponents<IDisposableComponent<IReadOnlyObservableCollection>>();
            components.OnDisposing(this, null);
            components.OnDisposed(this, null);
            this.ClearComponents();
            IsDisposed = true;
        }

        public bool Reset(IEnumerable<TValue>? items)
        {
            if (ReferenceEquals(items, this))
                return false;

            using var _ = Lock();
            if (items == null && _list.Count == 0 || !GetComponents<IConditionCollectionComponent<TValue>>().CanReset(this, items))
                return false;

            GetComponents<ICollectionChangingListener<TValue>>().OnResetting(this, items);
            _dictionary.Clear();
            _list.Clear();
            if (items != null)
                items = ResetInternal(items);

            GetComponents<ICollectionChangedListener<TValue>>().OnReset(this, _list.Count == 0 ? null : items);
            return true;
        }

        private IEnumerable<TValue> ResetInternal(IEnumerable<TValue> items)
        {
            var hasDup = false;
            foreach (var item in items)
            {
                var key = _getKey(item);
                if (_dictionary.ContainsKey(key))
                    hasDup = true;
                else
                    _dictionary[key] = _list.AddLast(item);
            }

            return hasDup ? _list : items;
        }

        private int IndexOf(LinkedListNode<TValue> node)
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

        private new ActionToken Lock() => Lock(true);

        private ActionToken Lock(bool checkDisposed)
        {
            var actionToken = base.Lock();
            if (checkDisposed && IsDisposed)
            {
                actionToken.Dispose();
                ExceptionManager.ThrowObjectDisposed(this);
            }

            return actionToken;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            using var _ = Lock();
            ((ICollection) _list).CopyTo(array, index);
        }

        void ICollection<TValue>.Add(TValue item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorRef(this);

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new EnumeratorRef(this);

        bool IHasComponentAddConditionHandler.CanAddComponent(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        void IHasFindAllIndexOfSupport.FindAllIndexOf(object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            TKey key;
            if (item is not TValue value)
            {
                if (item != null || !NullValueKey.HasValue || !TypeChecker.IsNullable<TValue>())
                    return;
                key = NullValueKey.Value!;
            }
            else
                key = _getKey(value);

            if (_dictionary.TryGetValue(key, out var node))
                indexes.Add(IndexOf(node));
        }

        bool IObservableCollection.Reset(IEnumerable? items) => Reset(items as IEnumerable<TValue> ?? items?.Cast<TValue>());

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<TValue>
        {
            private LinkedList<TValue>.Enumerator _enumerator;
            private ActionToken _locker;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ObservableSet<TKey, TValue> collection)
            {
                _enumerator = collection._list.GetEnumerator();
                _locker = collection.Lock();
            }

            public readonly TValue Current
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

        private sealed class EnumeratorRef : IEnumerator<TValue>
        {
            private LinkedList<TValue>.Enumerator _enumerator;
            private ActionToken _locker;

            internal EnumeratorRef(ObservableSet<TKey, TValue> collection)
            {
                _enumerator = collection._list.GetEnumerator();
                _locker = collection.Lock();
            }

            public TValue Current => _enumerator.Current!;

            object IEnumerator.Current => _enumerator.Current!;

            public void Dispose() => _locker.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => ((IEnumerator) _enumerator).Reset();
        }
    }
}