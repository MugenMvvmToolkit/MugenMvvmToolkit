using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

// ReSharper disable NonAtomicCompoundOperator

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(SynchronizedObservableCollection<>.DebuggerProxy))]
    public sealed class SynchronizedObservableCollection<T> : ComponentOwnerBase<IReadOnlyObservableCollection>, IObservableCollection<T>, IObservableCollection, ISynchronizable,
        IHasComponentAddedHandler, IHasComponentRemovedHandler, IHasComponentChangedHandler, IHasComponentAddingHandler
    {
        private const int DefaultCapacity = 4;
        private const int MaxArrayLength = 0X7FEFFFFF;

        private readonly ComponentTracker _componentTracker;

        private ILocker? _lastTakenLocker;
        private int _lockCount;
        private int _batchCount;
        private volatile int _state;
        private ILocker? _locker;

        private volatile int _size;
        private T[] _items;
        private ItemOrArray<ICollectionItemPreInitializerComponent<T>> _preInitializers;
        private ItemOrArray<IConditionCollectionComponent<T>> _conditions;
        private ItemOrArray<ICollectionChangingListener<T>> _changingListeners;
        private ItemOrArray<ICollectionChangedListener<T>> _changedListeners;
        private ItemOrArray<ICollectionBatchUpdateListener> _batchListeners;

        public SynchronizedObservableCollection(IComponentCollectionManager? componentCollectionManager = null) : this(null, componentCollectionManager)
        {
        }

        public SynchronizedObservableCollection(IEnumerable<T>? items, IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<ICollectionItemPreInitializerComponent<T>, SynchronizedObservableCollection<T>>(
                (components, state, _) => state._preInitializers = components, this);
            _componentTracker.AddListener<IConditionCollectionComponent<T>, SynchronizedObservableCollection<T>>((components, state, _) => state._conditions = components, this);
            _componentTracker.AddListener<ICollectionChangingListener<T>, SynchronizedObservableCollection<T>>((components, state, _) => state._changingListeners = components,
                this);
            _componentTracker.AddListener<ICollectionChangedListener<T>, SynchronizedObservableCollection<T>>((components, state, _) => state._changedListeners = components, this);
            _componentTracker.AddListener<ICollectionBatchUpdateListener, SynchronizedObservableCollection<T>>((components, state, _) => state._batchListeners = components, this);

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

        public bool IsDisposed => _state != 0;

        public bool IsReadOnly => false;

        public int Count => _size;

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

        ILocker ISynchronizable.Locker => GetLocker();

        public T this[int index]
        {
            get
            {
                using (Lock())
                {
                    if ((uint)index >= (uint)_size)
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                    return _items[index];
                }
            }
            set
            {
                _preInitializers.Initialize(this, value);
                using (Lock())
                {
                    if ((uint)index >= (uint)_size)
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                    var oldItem = _items[index];
                    if (EqualityComparer<T>.Default.Equals(value, oldItem))
                        return;

                    if (!_conditions.CanReplace(this, oldItem, value, index))
                        return;

                    _changingListeners.OnReplacing(this, oldItem, value, index);
                    _items[index] = value;
                    _changedListeners.OnReplaced(this, oldItem, value, index);
                }
            }
        }

        object? IList.this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        object? IObservableCollection.this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        object? IReadOnlyObservableCollection.this[int index] => Get(index);

        public Enumerator GetEnumerator() => new(this);

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
                Array.Copy(_items, 0, array, arrayIndex, _size);
            }
        }

        public void Add(T item)
        {
            _preInitializers.Initialize(this, item);
            using (Lock())
            {
                InsertInternal(_size, item, true);
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, int.MaxValue, 0) == 0)
                GetComponents<IDisposableComponent<IReadOnlyObservableCollection>>().Dispose(this, null);
        }

        public void RemoveAt(int index)
        {
            using (Lock())
            {
                if ((uint)index >= (uint)_size)
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
            _preInitializers.Initialize(this, item);
            using (Lock())
            {
                if ((uint)index > (uint)_size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                InsertInternal(index, item, false);
            }
        }

        public void Reset(IReadOnlyCollection<T>? items)
        {
            if (items != null && _preInitializers.Count != 0)
            {
                foreach (var item in items)
                    _preInitializers.Initialize(this, item);
            }

            using (Lock())
            {
                if (items == null && _size == 0 || !_conditions.CanReset(this, items))
                    return;

                _changingListeners.OnResetting(this, items);
                ClearRaw();
                if (items != null)
                    InsertRangeRaw(_size, items);
                _changedListeners.OnReset(this, items);
            }
        }

        public void RaiseItemChanged(T item, object? args)
        {
            using (Lock())
            {
                var index = IndexOfInternal(item);
                if (index != -1)
                    _changedListeners.OnChanged(this, item, index, args);
            }
        }

        public ActionToken BatchUpdate()
        {
            if (_batchListeners.Count == 0)
                return default;
            if (Interlocked.Increment(ref _batchCount) == 1)
                _batchListeners.OnBeginBatchUpdate(this, BatchUpdateType.Source);
            return ActionToken.FromDelegate((@this, _) => ((SynchronizedObservableCollection<T>)@this!).EndBatchUpdate(), this);
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            using (Lock())
            {
                if ((uint)oldIndex >= (uint)_size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(oldIndex));
                if ((uint)newIndex >= (uint)_size)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(newIndex));

                var obj = _items[oldIndex];
                if (!_conditions.CanMove(this, obj, oldIndex, newIndex))
                    return;

                _changingListeners.OnMoving(this, obj, oldIndex, newIndex);
                RemoveAtRaw(oldIndex);
                InsertRaw(newIndex, obj);
                _changedListeners.OnMoved(this, obj, oldIndex, newIndex);
            }
        }

        public ActionToken Lock()
        {
            var lockTaken = false;
            ILocker? locker = null;
            try
            {
                while (true)
                {
                    locker = _lastTakenLocker ?? GetLocker();
                    locker.Enter(ref lockTaken);

                    var currentLocker = _lastTakenLocker ?? GetLocker();
                    if (ReferenceEquals(currentLocker, locker))
                    {
                        if (lockTaken)
                        {
                            _lastTakenLocker = locker;
                            ++_lockCount;
                            return ActionToken.FromDelegate((c, l) => ((SynchronizedObservableCollection<T>)c!).Unlock((ILocker)l!), this, locker);
                        }

                        return default;
                    }

                    if (lockTaken)
                    {
                        lockTaken = false;
                        locker.Exit();
                    }
                }
            }
            catch
            {
                if (lockTaken && locker != null)
                    locker.Exit();
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCompatibleObject(object? value) => value is T || value == null && default(T) == null;

        private void InsertInternal(int index, T item, bool isAdd)
        {
            if (!_conditions.CanAdd(this, item, index))
                return;

            _changingListeners.OnAdding(this, item, index);
            if (isAdd)
                AddRaw(item);
            else
                InsertRaw(index, item);
            _changedListeners.OnAdded(this, item, index);
        }

        private void RemoveInternal(int index)
        {
            var oldItem = _items[index];
            if (!_conditions.CanRemove(this, oldItem, index))
                return;

            _changingListeners.OnRemoving(this, oldItem, index);
            RemoveAtRaw(index);
            _changedListeners.OnRemoved(this, oldItem, index);
        }

        private void AddRaw(T item)
        {
            var size = _size;
            if ((uint)size >= (uint)_items.Length)
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int IndexOfInternal(T? item) => Array.IndexOf(_items, item, 0, _size);

        private ILocker GetLocker()
        {
            lock (_componentTracker)
            {
                return _locker ??= new DecrementPriorityLocker();
            }
        }

        private void Unlock(ILocker locker)
        {
            if (--_lockCount == 0)
                _lastTakenLocker = null;
            locker.Exit();
        }

        private object? Get(int index) => BoxingExtensions.Box(this[index]);

        private void Set(int index, object? value) => this[index] = (T)value!;

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min)
                return;

            var newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if ((uint)newCapacity > MaxArrayLength)
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

        private void EndBatchUpdate()
        {
            if (Interlocked.Decrement(ref _batchCount) == 0)
                _batchListeners.OnEndBatchUpdate(this, BatchUpdateType.Source);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            Should.BeValid(array.Rank == 1, nameof(array));
            using (Lock())
            {
                Array.Copy(_items, 0, array, index, _size);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!IsDisposed)
                _componentTracker.OnComponentChanged(collection, component, metadata);
        }

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        void IHasComponentChangedHandler.OnComponentChanged(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!IsDisposed)
                _componentTracker.OnComponentChanged(collection, component, metadata);
        }

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!IsDisposed)
                _componentTracker.OnComponentChanged(collection, component, metadata);
        }

        int IList.Add(object? value)
        {
            using (Lock())
            {
                InsertInternal(_size, (T)value!, true);
                return _size - 1;
            }
        }

        bool IList.Contains(object? value) => IsCompatibleObject(value) && Contains((T)value!);

        int IList.IndexOf(object? value) => IsCompatibleObject(value!) ? IndexOf((T)value!) : -1;

        void IList.Insert(int index, object? value) => Insert(index, (T)value!);

        void IList.Remove(object? value)
        {
            if (IsCompatibleObject(value!))
                Remove((T)value!);
        }

        void IObservableCollection.Reset(IEnumerable? items) => Reset((IReadOnlyCollection<T>?)items);

        void IObservableCollection.RaiseItemChanged(object item, object? args)
        {
            if (IsCompatibleObject(item))
                RaiseItemChanged((T)item, args);
        }

        void ISynchronizable.UpdateLocker(ILocker locker)
        {
            Should.NotBeNull(locker, nameof(locker));
            if (ReferenceEquals(locker, _locker))
                return;

            var set = false;
            lock (_componentTracker)
            {
                if (_locker == null || locker.Priority > _locker.Priority)
                {
                    _locker = locker;
                    set = true;
                }
            }

            if (set)
                GetComponents<ILockerChangedListener<IReadOnlyObservableCollection>>().OnChanged(this, locker, null);
        }

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

        [Preserve(AllMembers = true)]
        internal sealed class DebuggerProxy
        {
            private readonly SynchronizedObservableCollection<T> _collection;

            public DebuggerProxy(SynchronizedObservableCollection<T> collection)
            {
                _collection = collection;
            }

            public IEnumerable<T> Items => _collection.ToArray();

            public IEnumerable<object?> DecoratedItems => _collection.Decorate().ToArray();
        }
    }
}