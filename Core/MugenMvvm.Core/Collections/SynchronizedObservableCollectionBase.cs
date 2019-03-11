using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public abstract class SynchronizedObservableCollectionBase<T, TItemCollection> : IList, IReadOnlyList<T>, IObservableCollection<T>
        where TItemCollection : class, IList<T>
    {
        #region Fields

        private int _batchCount;
        private IComponentCollection<IObservableCollectionChangedListener<T>>? _listeners;

        #endregion

        #region Constructors

        protected SynchronizedObservableCollectionBase(TItemCollection list, IComponentCollection<IObservableCollectionChangedListener<T>>? listeners = null)
        {
            Should.NotBeNull(list, nameof(list));
            Items = list;
            _listeners = listeners;
            Locker = new object();
        }

        #endregion

        #region Properties

        protected TItemCollection Items { get; }

        protected object Locker { get; }

        public int Count
        {
            get
            {
                lock (Locker)
                {
                    return GetCountInternal();
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (Locker)
                {
                    return GetInternal(index);
                }
            }
            set
            {
                lock (Locker)
                {
                    if (index < 0 || index >= GetCountInternal())
                        throw ExceptionManager.IndexOutOfRangeCollection(nameof(index));
                    SetInternal(index, value);
                }
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => Locker;

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        public IComponentCollection<IObservableCollectionChangedListener<T>> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        public IComponentCollection<IObservableCollectionDecorator<T>> Decorators { get; }

        public IComponentCollection<IObservableCollectionChangedListener<T>> DecoratorListeners { get; }

        #endregion

        #region Implementation of interfaces

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            lock (Locker)
            {
                CopyToInternal(array, index);
            }
        }

        int IList.Add(object value)
        {
            return Add((T)value);
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
                return IndexOf((T)value);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
                Remove((T)value);
        }

        public void Clear()
        {
            lock (Locker)
            {
                ClearInternal();
            }
        }

        public void RemoveAt(int index)
        {
            lock (Locker)
            {
                if (index < 0 || index >= GetCountInternal())
                    throw ExceptionManager.IndexOutOfRangeCollection(nameof(index));
                RemoveInternal(index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void CopyTo(T[] array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            lock (Locker)
            {
                CopyToInternal(array, index);
            }
        }

        public bool Contains(T item)
        {
            lock (Locker)
            {
                return ContainsInternal(item);
            }
        }

        public int IndexOf(T item)
        {
            lock (Locker)
            {
                return IndexOfInternal(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (Locker)
            {
                if (index < 0 || index > GetCountInternal())
                    throw ExceptionManager.IndexOutOfRangeCollection(nameof(index));
                InsertInternal(index, item, false);
            }
        }

        public bool Remove(T item)
        {
            lock (Locker)
            {
                var index = IndexOfInternal(item);
                if (index < 0)
                    return false;
                RemoveInternal(index);
                return true;
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            lock (Locker)
            {
                MoveInternal(oldIndex, newIndex);
            }
        }

        public IEnumerable<T> DecorateItems()
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginBatchUpdate()
        {
            if (Interlocked.Increment(ref _batchCount) == 1)
                OnBeginBatchUpdate();
            return WeakActionToken.Create(this, @base => @base.EndBatchUpdate());
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Methods

        public int Add(T item)
        {
            lock (Locker)
            {
                return InsertInternal(GetCountInternal(), item, true);
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        protected virtual void CopyToInternal(Array array, int index)
        {
            var genericArray = array as T[];
            var count = GetCountInternal();
            if (genericArray == null)
            {
                for (var i = index; i < count; i++)
                {
                    if (i >= array.Length)
                        break;
                    array.SetValue(GetInternal(i), i);
                }
            }
            else
            {
                for (var i = index; i < count; i++)
                {
                    if (i >= genericArray.Length)
                        break;
                    genericArray[i] = GetInternal(i);
                }
            }
        }

        protected virtual int GetCountInternal()
        {
            return Items.Count;
        }

        protected int IndexOfInternal(T item)
        {
            return Items.IndexOf(item);
        }

        protected bool ContainsInternal(T item)
        {
            return Items.Contains(item);
        }

        protected virtual void MoveInternal(int oldIndex, int newIndex)
        {
            var obj = Items[oldIndex];
            if (!OnMoving(obj, oldIndex, newIndex))
                return;
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            OnMoved(obj, oldIndex, newIndex);
        }

        protected virtual void ClearInternal()
        {
            if (!OnClearing())
                return;
            Items.Clear();
            OnCleared();
        }

        protected virtual int InsertInternal(int index, T item, bool isAdd)
        {
            if (!OnAdding(item, index))
                return -1;
            Items.Insert(index, item);
            OnAdded(item, index);
            return index;
        }

        protected virtual void RemoveInternal(int index)
        {
            var oldItem = Items[index];
            if (!OnRemoving(oldItem, index))
                return;
            Items.RemoveAt(index);
            OnRemoved(oldItem, index);
        }

        protected virtual T GetInternal(int index)
        {
            return Items[index];
        }

        protected virtual void SetInternal(int index, T item)
        {
            var oldItem = Items[index];
            if (!OnReplacing(oldItem, item, index))
                return;
            Items[index] = item;
            OnReplaced(oldItem, item, index);
        }

        protected virtual void OnBeginBatchUpdate()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnBeginBatchUpdate(this);
        }

        protected virtual void OnEndBatchUpdate()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnEndBatchUpdate(this);
        }

        protected bool OnAdding(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnAdding(this, item, index))
                    return false;
            }

            return true;
        }

        protected bool OnReplacing(T oldItem, T newItem, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnReplacing(this, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        protected bool OnMoving(T item, int oldIndex, int newIndex)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnMoving(this, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        protected bool OnRemoving(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnRemoving(this, item, index))
                    return false;
            }

            return true;
        }

        protected bool OnResetting()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnResetting(this))
                    return false;
            }

            return true;
        }

        protected bool OnClearing()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener<T> listener && !listener.OnClearing(this))
                    return false;
            }

            return true;
        }

        protected void OnAdded(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnAdded(this, item, index);
        }

        protected void OnReplaced(T oldItem, T newItem, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnReplaced(this, oldItem, newItem, index);
        }

        protected void OnMoved(T item, int oldIndex, int newIndex)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnMoved(this, item, oldIndex, newIndex);
        }

        protected void OnRemoved(T item, int index)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnRemoved(this, item, index);
        }

        protected void OnReset()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnReset(this);
        }

        protected void OnCleared()
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnCleared(this);
        }

        protected IReadOnlyList<IObservableCollectionChangedListener<T>> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IObservableCollectionChangedListener<T>>();
        }

        private void EndBatchUpdate()
        {
            if (Interlocked.Decrement(ref _batchCount) == 0)
                OnEndBatchUpdate();
        }

        private static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;
            if (value == null)
                return default(T) == null;
            return false;
        }

        #endregion

        #region Nested types

        public struct Enumerator : IEnumerator<T>
        {
            #region Fields

            private readonly SynchronizedObservableCollectionBase<T, TItemCollection> _collection;
            private int _index;

            #endregion

            #region Constructors

            public Enumerator(SynchronizedObservableCollectionBase<T, TItemCollection> collection)
            {
                _collection = collection;
                _index = 0;
                Current = default;
            }

            #endregion

            #region Properties

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            #endregion

            #region Implementation of interfaces

            public bool MoveNext()
            {
                if (_collection == null)
                    return false;

                lock (_collection.Locker)
                {
                    if (_index >= _collection.GetCountInternal())
                        return false;

                    Current = _collection.GetInternal(_index);
                    ++_index;
                }

                return true;
            }

            public void Reset()
            {
                _index = 0;
            }

            public void Dispose()
            {
            }

            #endregion
        }

        #endregion
    }
}