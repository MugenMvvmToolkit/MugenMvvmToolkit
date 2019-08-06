using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public class SynchronizedObservableCollection<T> : ObservableCollectionBase<T>, IList
    {
        #region Fields

        private static readonly bool IsValueType = typeof(T).IsValueTypeUnified();

        #endregion

        #region Constructors

        protected SynchronizedObservableCollection(IList<T> list, IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
            Should.NotBeNull(list, nameof(list));
            Items = list;
            Locker = new LockerImpl();
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

        public sealed override int Count
        {
            get
            {
                lock (Locker)
                {
                    return GetCountInternal();
                }
            }
        }

        protected LockerImpl Locker { get; }

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => Locker;

        public sealed override bool IsReadOnly => false;

        object IList.this[int index]
        {
            get => this[index]!;
            set => this[index] = (T)value;
        }

        public sealed override T this[int index]
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
                InsertInternal(GetCountInternal(), (T)value, true);
                return GetCountInternal() - 1;
            }
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

        public sealed override void RemoveAt(int index)
        {
            lock (Locker)
            {
                if (index < 0 || index >= GetCountInternal())
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                RemoveInternal(index);
            }
        }

        public sealed override void Clear()
        {
            lock (Locker)
            {
                ClearInternal();
            }
        }

        #endregion

        #region Methods

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public sealed override bool Remove(T item)
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

        public sealed override int IndexOf(T item)
        {
            lock (Locker)
            {
                return IndexOfInternal(item);
            }
        }

        public sealed override void Insert(int index, T item)
        {
            lock (Locker)
            {
                if (index < 0 || index > GetCountInternal())
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                InsertInternal(index, item, false);
            }
        }

        public sealed override void Move(int oldIndex, int newIndex)
        {
            lock (Locker)
            {
                MoveInternal(oldIndex, newIndex);
            }
        }

        public override void Reset(IEnumerable<T> items)
        {
            Should.NotBeNull(items, nameof(items));
            lock (Locker)
            {
                ResetInternal(items);
            }
        }

        public sealed override void RaiseItemChanged(T item, object? args)
        {
            lock (Locker)
            {
                var index = IndexOfInternal(item);
                if (index >= 0)
                    OnItemChanged(null, item, index, args);
            }
        }

        public override IDisposable Lock()
        {
            return Locker.Lock();
        }

        public sealed override bool Contains(T item)
        {
            lock (Locker)
            {
                return ContainsInternal(item);
            }
        }

        public sealed override void CopyTo(T[] array, int arrayIndex)
        {
            Should.NotBeNull(array, nameof(array));
            lock (Locker)
            {
                CopyToInternal(array, arrayIndex);
            }
        }

        public sealed override void Add(T item)
        {
            lock (Locker)
            {
                InsertInternal(GetCountInternal(), item, true);
            }
        }

        protected sealed override IEnumerator<T> GetEnumeratorInternal()
        {
            return GetEnumerator();
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
            if (oldIndex == newIndex)
                return;

            var obj = Items[oldIndex];
            if (!OnMoving(obj, oldIndex, newIndex))
                return;
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            OnMoved(obj, oldIndex, newIndex);
        }

        protected virtual void ClearInternal()
        {
            if (GetCountInternal() == 0)
                return;

            if (!OnClearing())
                return;
            Items.Clear();
            OnCleared();
        }

        protected virtual void ResetInternal(IEnumerable<T> items)
        {
            if (!OnResetting(items))
                return;

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            OnReset(this);
        }

        protected virtual void InsertInternal(int index, T item, bool isAdd)
        {
            if (!OnAdding(item, index))
                return;

            Items.Insert(index, item);
            OnAdded(item, index);
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
            if (!IsValueType && ReferenceEquals(oldItem, item))
                return;
            if (!OnReplacing(oldItem, item, index))
                return;
            Items[index] = item;
            OnReplaced(oldItem, item, index);
        }

        #endregion

        #region Nested types

        protected sealed class LockerImpl : IDisposable
        {
            #region Implementation of interfaces

            public void Dispose()
            {
                Monitor.Exit(this);
            }

            #endregion

            #region Methods

            public IDisposable Lock()
            {
                Monitor.Enter(this);
                return this;
            }

            #endregion
        }

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