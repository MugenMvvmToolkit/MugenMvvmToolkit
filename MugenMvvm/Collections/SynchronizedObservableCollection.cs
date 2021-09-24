using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
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
    public sealed class SynchronizedObservableCollection<T> : SynchronizableComponentOwnerBase<IReadOnlyObservableCollection>, IObservableCollection<T>, IObservableCollection,
        IHasComponentAddConditionHandler
    {
        private const int DefaultCapacity = 4;
        private const int MaxArrayLength = 0X7FEFFFFF;

        private int _size;
        private T[] _items;

        public SynchronizedObservableCollection(IComponentCollectionManager? componentCollectionManager = null) : this(null, componentCollectionManager)
        {
        }

        public SynchronizedObservableCollection(int capacity, IComponentCollectionManager? componentCollectionManager = null) : this(null, componentCollectionManager)
        {
            if (capacity > 0)
                _items = new T[capacity];
        }

        public SynchronizedObservableCollection(IEnumerable<T>? items, IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            if (items == null)
                _items = Array.Empty<T>();
            else if (items is ICollection<T> c)
            {
                var count = c.Count;
                if (count == 0)
                    _items = Array.Empty<T>();
                else
                {
                    _items = new T[count];
                    c.CopyTo(_items, 0);
                    _size = count;
                }
            }
            else
            {
                _items = Array.Empty<T>();
                foreach (var item in items)
                    Add(item);
            }
        }

        public bool IsReadOnly => false;

        public bool IsDisposed { get; private set; }

        public int Count => Volatile.Read(ref _size);

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot
        {
            get
            {
                ExceptionManager.ThrowNotSupported(nameof(ICollection.SyncRoot));
                return null;
            }
        }

        bool IList.IsFixedSize => false;

        Type IReadOnlyObservableCollection.ItemType => typeof(T);

        public T this[int index]
        {
            get
            {
                using (Lock())
                {
                    EnsureNotDisposed();
                    if ((uint) index >= (uint) _size)
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                    return _items[index];
                }
            }
            set
            {
                using (Lock())
                {
                    EnsureNotDisposed();
                    if ((uint) index >= (uint) _size)
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                    var oldItem = _items[index];
                    if (EqualityComparer<T>.Default.Equals(value, oldItem))
                        return;

                    if (!GetComponents<IConditionCollectionComponent<T>>().CanReplace(this, oldItem, value, index))
                        return;

                    GetComponents<ICollectionChangingListener<T>>().OnReplacing(this, oldItem, value, index);
                    _items[index] = value;
                    GetComponents<ICollectionChangedListener<T>>().OnReplaced(this, oldItem, value, index);
                }
            }
        }

        object? IList.this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            EnsureNotDisposed();
            return new Enumerator(this);
        }

        public void Clear() => Reset(null);

        public bool Remove(T? item)
        {
            using (Lock())
            {
                var index = IndexOfInternal(item);
                if (index < 0)
                    return false;
                RemoveInternal(index);
                return true;
            }
        }

        public bool Contains(T? item)
        {
            using (Lock())
            {
                return _size != 0 && IndexOfInternal(item) != -1;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Should.NotBeNull(array, nameof(array));
            using (Lock())
            {
                EnsureNotDisposed();
                Array.Copy(_items, 0, array, arrayIndex, _size);
            }
        }

        public void Add(T item)
        {
            using (Lock())
            {
                InsertInternal(_size, item, true);
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

        public void RemoveAt(int index)
        {
            using (Lock())
            {
                if ((uint) index >= (uint) _size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                RemoveInternal(index);
            }
        }

        public int IndexOf(T? item)
        {
            using (Lock())
            {
                return IndexOfInternal(item);
            }
        }

        public void Insert(int index, T item)
        {
            using (Lock())
            {
                if ((uint) index > (uint) _size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                InsertInternal(index, item, false);
            }
        }

        public void Reset(IEnumerable<T>? items)
        {
            if (ReferenceEquals(items, this))
                return;

            using (Lock())
            {
                EnsureNotDisposed();
                if (items == null && _size == 0 || !GetComponents<IConditionCollectionComponent<T>>().CanReset(this, items))
                    return;

                GetComponents<ICollectionChangingListener<T>>().OnResetting(this, items);
                ClearRaw();
                if (items != null)
                    InsertRangeRaw(_size, items);
                GetComponents<ICollectionChangedListener<T>>().OnReset(this, _size == 0 ? null : items);
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            using (Lock())
            {
                EnsureNotDisposed();
                if ((uint) oldIndex >= (uint) _size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(oldIndex));
                if ((uint) newIndex >= (uint) _size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(newIndex));

                var obj = _items[oldIndex];
                if (!GetComponents<IConditionCollectionComponent<T>>().CanMove(this, obj, oldIndex, newIndex))
                    return;

                GetComponents<ICollectionChangingListener<T>>().OnMoving(this, obj, oldIndex, newIndex);
                if (newIndex < oldIndex)
                    Array.Copy(_items, newIndex, _items, newIndex + 1, oldIndex - newIndex);
                else
                    Array.Copy(_items, oldIndex + 1, _items, oldIndex, newIndex - oldIndex);
                _items[newIndex] = obj;
                GetComponents<ICollectionChangedListener<T>>().OnMoved(this, obj, oldIndex, newIndex);
            }
        }

        private void InsertInternal(int index, T item, bool isAdd)
        {
            EnsureNotDisposed();
            if (!GetComponents<IConditionCollectionComponent<T>>().CanAdd(this, item, index))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnAdding(this, item, index);
            if (isAdd)
                AddRaw(item);
            else
                InsertRaw(index, item);
            GetComponents<ICollectionChangedListener<T>>().OnAdded(this, item, index);
        }

        private void RemoveInternal(int index)
        {
            EnsureNotDisposed();
            var oldItem = _items[index];
            if (!GetComponents<IConditionCollectionComponent<T>>().CanRemove(this, oldItem, index))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnRemoving(this, oldItem, index);
            RemoveAtRaw(index);
            GetComponents<ICollectionChangedListener<T>>().OnRemoved(this, oldItem, index);
        }

        private void AddRaw(T item)
        {
            var size = _size;
            if ((uint) size >= (uint) _items.Length)
                EnsureCapacity(size + 1);

            _size = size + 1;
            _items[size] = item;
        }

        private void InsertRaw(int index, T item)
        {
            if (_size == _items.Length)
                EnsureCapacity(_size + 1);
            if (index < _size)
                Array.Copy(_items, index, _items, index + 1, _size - index);
            _items[index] = item;
            _size++;
        }

        private void InsertRangeRaw(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> c)
            {
                var count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(_size + count);
                    if (index < _size)
                        Array.Copy(_items, index, _items, index + count, _size - index);

                    if (ReferenceEquals(this, c))
                    {
                        Array.Copy(_items, 0, _items, index, index);
                        Array.Copy(_items, index + count, _items, index * 2, _size - index);
                    }
                    else
                        c.CopyTo(_items, index);

                    _size += count;
                }
            }
            else
            {
                foreach (var item in collection)
                    InsertRaw(index++, item);
            }
        }

        private void RemoveAtRaw(int index)
        {
            _size--;
            if (index < _size)
                Array.Copy(_items, index + 1, _items, index, _size - index);
#if !NET461
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
#endif
                _items[_size] = default!;
        }

        private void ClearRaw()
        {
#if !NET461
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
#endif
            {
                var size = _size;
                _size = 0;
                if (size > 0)
                    Array.Clear(_items, 0, size);
            }
#if !NET461
            else
                _size = 0;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int IndexOfInternal(T? item)
        {
            EnsureNotDisposed();
            return Array.IndexOf(_items, item, 0, _size);
        }

        private object? Get(int index) => BoxingExtensions.Box(this[index]);

        private void Set(int index, object? value) => this[index] = (T) value!;

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min)
                return;

            var newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if ((uint) newCapacity > MaxArrayLength)
                newCapacity = MaxArrayLength;
            if (newCapacity < min)
                newCapacity = min;
            SetCapacity(newCapacity);
        }

        private void SetCapacity(int value)
        {
            if (value == _items.Length)
                return;

            if (value > 0)
            {
                T[] newItems = new T[value];
                if (_size > 0)
                    Array.Copy(_items, newItems, _size);
                _items = newItems;
            }
            else
                _items = Array.Empty<T>();
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

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            Should.BeValid(array.Rank == 1, nameof(array));
            using (Lock())
            {
                EnsureNotDisposed();
                Array.Copy(_items, 0, array, index, _size);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorRef();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumeratorRef();

        bool IHasComponentAddConditionHandler.CanAddComponent(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        int IList.Add(object? value)
        {
            using (Lock())
            {
                InsertInternal(_size, (T) value!, true);
                return _size - 1;
            }
        }

        bool IList.Contains(object? value) => TypeChecker.IsCompatible<T>(value) && Contains((T) value!);

        int IList.IndexOf(object? value) => TypeChecker.IsCompatible<T>(value!) ? IndexOf((T) value!) : -1;

        void IList.Insert(int index, object? value) => Insert(index, (T) value!);

        void IList.Remove(object? value)
        {
            if (TypeChecker.IsCompatible<T>(value!))
                Remove((T) value!);
        }

        void IObservableCollection.Reset(IEnumerable? items) => Reset((IEnumerable<T>?) items);

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly SynchronizedObservableCollection<T>? _collection;
            private ActionToken _locker;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(SynchronizedObservableCollection<T> collection)
            {
                _collection = collection;
                _index = -1;
                _locker = collection.Lock();
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _collection == null ? default! : _collection._items[_index];
            }

            object IEnumerator.Current => Current!;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _collection != null && ++_index < _collection._size;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _locker.Dispose();
        }

        private sealed class EnumeratorRef : IEnumerator<T>
        {
            private readonly SynchronizedObservableCollection<T>? _collection;
            private ActionToken _locker;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal EnumeratorRef(SynchronizedObservableCollection<T> collection)
            {
                _collection = collection;
                _index = -1;
                _locker = collection.Lock();
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _collection == null ? default! : _collection._items[_index];
            }

            object IEnumerator.Current => Current!;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _locker.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _collection != null && ++_index < _collection._size;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;
        }
    }
}