using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Infrastructure;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Collections
{
    [Serializable]
    public abstract class SynchronizedObservableCollectionBase<T, TItems> : HasListenersBase<IObservableCollectionChangedListener>,
        IList, IReadOnlyList<T>, IObservableCollection<T>
        where TItems : class, IList<T>//todo add decorators
    {
        #region Fields

        [NonSerialized]
        private int _batchCount;

        #endregion

        #region Constructors

        protected SynchronizedObservableCollectionBase(TItems list)
        {
            Should.NotBeNull(list, nameof(list));
            Items = list;
            Locker = new object();
        }

        #endregion

        #region Properties

        protected TItems Items { get; }

        [field: NonSerialized]
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

        protected virtual void OnBeginBatchUpdate()
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnBeginBatchUpdate(this);
            }
        }

        protected virtual void OnEndBatchUpdate()
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnEndBatchUpdate(this);
            }
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
            if (HasListeners && !OnMoving(obj, oldIndex, newIndex))
                return;
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            if (HasListeners)
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
            if (HasListeners && !OnAdding(item, index))
                return -1;
            Items.Insert(index, item);
            if (HasListeners)
                OnAdded(item, index);
            return index;
        }

        protected virtual void RemoveInternal(int index)
        {
            var oldItem = Items[index];
            if (HasListeners && !OnRemoving(oldItem, index))
                return;
            Items.RemoveAt(index);
            if (HasListeners)
                OnRemoved(oldItem, index);
        }

        protected virtual T GetInternal(int index)
        {
            return Items[index];
        }

        protected virtual void SetInternal(int index, T item)
        {
            var oldItem = Items[index];
            if (HasListeners && !OnReplacing(oldItem, item, index))
                return;
            Items[index] = item;
            if (HasListeners)
                OnReplaced(oldItem, item, index);
        }

        protected bool OnAdding(object item, int index)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return true;
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener listener && !listener.OnAdding(this, item, index))
                    return false;
            }

            return true;
        }

        protected bool OnReplacing(object oldItem, object newItem, int index)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return true;
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener listener && !listener.OnReplacing(this, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        protected bool OnMoving(object item, int oldIndex, int newIndex)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return true;
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener listener && !listener.OnMoving(this, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        protected bool OnRemoving(object item, int index)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return true;
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener listener && !listener.OnRemoving(this, item, index))
                    return false;
            }

            return true;
        }

        protected bool OnClearing()
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return true;
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] is IObservableCollectionChangingListener listener && !listener.OnClearing(this))
                    return false;
            }

            return true;
        }

        protected void OnAdded(object item, int index)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnAdded(this, item, index);
            }
        }

        protected void OnReplaced(object oldItem, object newItem, int index)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnReplaced(this, oldItem, newItem, index);
            }
        }

        protected void OnMoved(object item, int oldIndex, int newIndex)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnMoved(this, item, oldIndex, newIndex);
            }
        }

        protected void OnRemoved(object item, int index)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnRemoved(this, item, index);
            }
        }

        protected void OnCleared()
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    (listeners[i] as IObservableCollectionChangedListener)?.OnCleared(this);
            }
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

            private readonly SynchronizedObservableCollectionBase<T, TItems> _collection;
            private int _index;

            #endregion

            #region Constructors

            public Enumerator(SynchronizedObservableCollectionBase<T, TItems> collection)
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
                    return true;
                }
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