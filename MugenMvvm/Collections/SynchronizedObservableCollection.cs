using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    public class SynchronizedObservableCollection<T> : ComponentOwnerBase<ICollection>, IObservableCollection<T>, IObservableCollection, IReadOnlyList<T>
    {
        private int _batchCount;

        public SynchronizedObservableCollection(IEnumerable<T> items, IComponentCollectionManager? componentCollectionManager = null)
            : this(new List<T>(items), componentCollectionManager)
        {
        }

        public SynchronizedObservableCollection(IComponentCollectionManager? componentCollectionManager = null)
            : this(new List<T>(), componentCollectionManager)
        {
        }

        protected SynchronizedObservableCollection(IList<T> list, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            Should.NotBeNull(list, nameof(list));
            Items = list;
            Locker = new object();
        }

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

        public bool IsReadOnly => false;

        protected IList<T> Items { get; }

        protected object Locker { get; }

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => Locker;

        bool IList.IsFixedSize => false;

        Type IObservableCollectionBase.ItemType => typeof(T);

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

        object? IList.this[int index]
        {
            get => BoxingExtensions.Box(this[index])!;
            set => this[index] = (T) value!;
        }

        protected static bool IsCompatibleObject(object? value)
        {
            if (value is T)
                return true;

            if (value == null)
                return TypeChecker.IsNullable<T>();

            return false;
        }

        public Enumerator GetEnumerator() => new(this);

        public void Clear()
        {
            lock (Locker)
            {
                ResetInternal(null);
            }
        }

        public bool Remove(T? item)
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

        public bool Contains(T? item)
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

        public void RemoveAt(int index)
        {
            lock (Locker)
            {
                if (index < 0 || index >= GetCountInternal())
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                RemoveInternal(index);
            }
        }

        public int IndexOf(T? item)
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
                    GetComponents<ICollectionChangedListener<T>>().OnItemChanged(this, item, index, args);
            }
        }

        public ActionToken BeginBatchUpdate()
        {
            lock (Locker)
            {
                if (++_batchCount == 1)
                    GetComponents<ICollectionBatchUpdateListener>().OnBeginBatchUpdate(this);
            }

            return new ActionToken((@this, _) => ((SynchronizedObservableCollection<T>) @this!).EndBatchUpdate(), this);
        }

        public void Move(int oldIndex, int newIndex)
        {
            lock (Locker)
            {
                MoveInternal(oldIndex, newIndex);
            }
        }

        protected virtual void CopyToInternal(Array array, int index) => ((ICollection) Items).CopyTo(array, index);

        protected virtual void CopyToInternal(T[] array, int index) => Items.CopyTo(array, index);

        protected virtual int GetCountInternal() => Items.Count;

        protected virtual void MoveInternal(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            var obj = Items[oldIndex];
            if (!GetComponents<IConditionCollectionComponent<T>>().CanMove(this, obj, oldIndex, newIndex))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnMoving(this, obj, oldIndex, newIndex);
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            GetComponents<ICollectionChangedListener<T>>().OnMoved(this, obj, oldIndex, newIndex);
        }

        protected virtual void ResetInternal(IEnumerable<T>? items)
        {
            if (items == null && GetCountInternal() == 0 || !GetComponents<IConditionCollectionComponent<T>>().CanReset(this, items))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnResetting(this, items);
            Items.Clear();
            if (items != null)
                Items.AddRange(items);
            GetComponents<ICollectionChangedListener<T>>().OnReset(this, items);
        }

        protected virtual void InsertInternal(int index, T item, bool isAdd)
        {
            if (!GetComponents<IConditionCollectionComponent<T>>().CanAdd(this, item, index))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnAdding(this, item, index);
            Items.Insert(index, item);
            GetComponents<ICollectionChangedListener<T>>().OnAdded(this, item, index);
        }

        protected virtual void RemoveInternal(int index)
        {
            var oldItem = Items[index];
            if (!GetComponents<IConditionCollectionComponent<T>>().CanRemove(this, oldItem, index))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnRemoving(this, oldItem, index);
            Items.RemoveAt(index);
            GetComponents<ICollectionChangedListener<T>>().OnRemoved(this, oldItem, index);
        }

        protected virtual T GetInternal(int index) => Items[index];

        protected virtual void SetInternal(int index, T item)
        {
            var oldItem = Items[index];
            if (EqualityComparer<T>.Default.Equals(item, oldItem))
                return;

            if (!GetComponents<IConditionCollectionComponent<T>>().CanReplace(this, oldItem, item, index))
                return;

            GetComponents<ICollectionChangingListener<T>>().OnReplacing(this, oldItem, item, index);
            Items[index] = item;
            GetComponents<ICollectionChangedListener<T>>().OnReplaced(this, oldItem, item, index);
        }

        protected int IndexOfInternal(T? item) => Items.IndexOf(item!);

        protected bool ContainsInternal(T? item) => Items.Contains(item!);

        private void EndBatchUpdate()
        {
            lock (Locker)
            {
                if (--_batchCount == 0)
                    GetComponents<ICollectionBatchUpdateListener>().OnEndBatchUpdate(this);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            lock (Locker)
            {
                CopyToInternal(array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        int IList.Add(object? value)
        {
            lock (Locker)
            {
                InsertInternal(GetCountInternal(), (T) value!, true);
                return GetCountInternal() - 1;
            }
        }

        bool IList.Contains(object? value)
        {
            if (IsCompatibleObject(value))
                return Contains((T) value!);
            return false;
        }

        int IList.IndexOf(object? value)
        {
            if (IsCompatibleObject(value!))
                return IndexOf((T) value!);
            return -1;
        }

        void IList.Insert(int index, object? value) => Insert(index, (T) value!);

        void IList.Remove(object? value)
        {
            if (IsCompatibleObject(value!))
                Remove((T) value!);
        }

        void IObservableCollection.Reset(IEnumerable<object> items) => Reset((IEnumerable<T>) items);

        void IObservableCollection.RaiseItemChanged(object item, object? args) => RaiseItemChanged((T) item, args);

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly SynchronizedObservableCollection<T>? _collection;
            private int _index;

            public Enumerator(SynchronizedObservableCollection<T> collection)
            {
                _collection = collection;
                _index = 0;
                Current = default!;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current!;

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

            public void Reset() => _index = 0;

            public void Dispose()
            {
            }
        }
    }
}