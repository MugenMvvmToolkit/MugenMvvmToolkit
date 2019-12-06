using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public class SynchronizedObservableCollection<T> : ComponentOwnerBase<IObservableCollection<T>>, IObservableCollection<T>, IList
    {
        #region Fields

        private int _batchCount;

        #endregion

        #region Constructors

        protected SynchronizedObservableCollection(IList<T> list, IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            Should.NotBeNull(list, nameof(list));
            Items = list;
            Locker = new object();
        }

        public SynchronizedObservableCollection(IEnumerable<T> items, IComponentCollectionProvider? componentCollectionProvider = null)
            : this(new List<T>(items), componentCollectionProvider)
        {
        }

        public SynchronizedObservableCollection(IComponentCollectionProvider? componentCollectionProvider = null)
            : this(new List<T>(), componentCollectionProvider)
        {
        }

        #endregion

        #region Properties

        protected IList<T> Items { get; }

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

        protected object Locker { get; }

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => Locker;

        public bool IsReadOnly => false;

        object IList.this[int index]
        {
            get => this[index]!;
            set => this[index] = (T) value;
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
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                    SetInternal(index, value);
                }
            }
        }

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
            lock (Locker)
            {
                InsertInternal(GetCountInternal(), (T) value, true);
                return GetCountInternal() - 1;
            }
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
                return Contains((T) value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
                return IndexOf((T) value);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T) value);
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
                Remove((T) value);
        }

        public void RemoveAt(int index)
        {
            lock (Locker)
            {
                if (index < 0 || index >= GetCountInternal())
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                RemoveInternal(index);
            }
        }

        public void Clear()
        {
            lock (Locker)
            {
                ClearInternal();
            }
        }

        public ActionToken BeginBatchUpdate()
        {
            lock (Locker)
            {
                if (++_batchCount == 1)
                    MugenExtensions.ObservableCollectionOnBeginBatchUpdate(this);
            }

            return new ActionToken((@this, _) => ((SynchronizedObservableCollection<T>) @this).EndBatchUpdate(), this);
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
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                InsertInternal(index, item, false);
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            lock (Locker)
            {
                MoveInternal(oldIndex, newIndex);
            }
        }

        public void Reset(IEnumerable<T> items)
        {
            Should.NotBeNull(items, nameof(items));
            lock (Locker)
            {
                ResetInternal(items);
            }
        }

        public void RaiseItemChanged(T item, object? args)
        {
            lock (Locker)
            {
                var index = IndexOfInternal(item);
                if (index >= 0)
                    MugenExtensions.ObservableCollectionOnItemChanged(this, null, item, index, args);
            }
        }

        public bool Contains(T item)
        {
            lock (Locker)
            {
                return ContainsInternal(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Should.NotBeNull(array, nameof(array));
            lock (Locker)
            {
                CopyToInternal(array, arrayIndex);
            }
        }

        public void Add(T item)
        {
            lock (Locker)
            {
                InsertInternal(GetCountInternal(), item, true);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Methods

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        protected virtual void CopyToInternal(Array array, int index)
        {
            ((ICollection) Items).CopyTo(array, index);
        }

        protected virtual void CopyToInternal(T[] array, int index)
        {
            Items.CopyTo(array, index);
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
            if (oldIndex == newIndex)
                return;

            var obj = Items[oldIndex];
            if (!MugenExtensions.ObservableCollectionOnMoving(this, obj, oldIndex, newIndex))
                return;
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            MugenExtensions.ObservableCollectionOnMoved(this, obj, oldIndex, newIndex);
        }

        protected virtual void ClearInternal()
        {
            if (GetCountInternal() == 0)
                return;

            if (!MugenExtensions.ObservableCollectionOnClearing(this))
                return;
            Items.Clear();
            MugenExtensions.ObservableCollectionOnCleared(this);
        }

        protected virtual void ResetInternal(IEnumerable<T> items)
        {
            if (!MugenExtensions.ObservableCollectionOnResetting(this, items))
                return;

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            MugenExtensions.ObservableCollectionOnReset(this, this);
        }

        protected virtual void InsertInternal(int index, T item, bool isAdd)
        {
            if (!MugenExtensions.ObservableCollectionOnAdding(this, item, index))
                return;

            Items.Insert(index, item);
            MugenExtensions.ObservableCollectionOnAdded(this, item, index);
        }

        protected virtual void RemoveInternal(int index)
        {
            var oldItem = Items[index];
            if (!MugenExtensions.ObservableCollectionOnRemoving(this, oldItem, index))
                return;
            Items.RemoveAt(index);
            MugenExtensions.ObservableCollectionOnRemoved(this, oldItem, index);
        }

        protected virtual T GetInternal(int index)
        {
            return Items[index];
        }

        protected virtual void SetInternal(int index, T item)
        {
            var oldItem = Items[index];
            if (Default.IsNullable<T>() && ReferenceEquals(oldItem, item))
                return;
            if (!MugenExtensions.ObservableCollectionOnReplacing(this, oldItem, item, index))
                return;
            Items[index] = item;
            MugenExtensions.ObservableCollectionOnReplaced(this, oldItem, item, index);
        }

        protected static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;

            if (value == null)
                return Default.IsNullable<T>();

            return false;
        }

        private void EndBatchUpdate()
        {
            lock (Locker)
            {
                if (--_batchCount == 0)
                    MugenExtensions.ObservableCollectionOnEndBatchUpdate(this);
            }
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            #region Fields

            private readonly SynchronizedObservableCollection<T> _collection;
            private int _index;

            #endregion

            #region Constructors

            public Enumerator(SynchronizedObservableCollection<T> collection)
            {
                _collection = collection;
                _index = 0;
                Current = default!;
            }

            #endregion

            #region Properties

            public T Current { get; private set; }

            object IEnumerator.Current => Current!;

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