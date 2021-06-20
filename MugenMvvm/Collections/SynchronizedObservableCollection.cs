using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(SynchronizedObservableCollection<>.DebuggerProxy))]
    public class SynchronizedObservableCollection<T> : ComponentOwnerBase<ICollection>, IObservableCollection<T>, IObservableCollection, ISynchronizable
    {
        private readonly object _lockLocker;
        private object? _lastTakenLocker;
        private int _lockCount;
        private int _batchCount;
        private ILocker _locker;

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
            _lockLocker = new object();
            _locker = new DecrementPriorityLocker();
        }

        public bool IsReadOnly => false;

        public int Count
        {
            get
            {
                using (Lock())
                {
                    return GetCountInternal();
                }
            }
        }

        protected IList<T> Items { get; }

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => GetLocker();

        bool IList.IsFixedSize => false;

        Type IReadOnlyObservableCollection.ItemType => typeof(T);

        ILocker ISynchronizable.Locker => _locker;

        public T this[int index]
        {
            get
            {
                using (Lock())
                {
                    return GetInternal(index);
                }
            }
            set
            {
                using (Lock())
                {
                    if (index < 0 || index >= GetCountInternal())
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                    SetInternal(index, value);
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
            using (Lock())
            {
                ResetInternal(null);
            }
        }

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
                return ContainsInternal(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Should.NotBeNull(array, nameof(array));
            using (Lock())
            {
                CopyToInternal(array, arrayIndex);
            }
        }

        public void Add(T item)
        {
            using (Lock())
            {
                InsertInternal(GetCountInternal(), item, true);
            }
        }

        public void RemoveAt(int index)
        {
            using (Lock())
            {
                if (index < 0 || index >= GetCountInternal())
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
                if (index < 0 || index > GetCountInternal())
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                InsertInternal(index, item, false);
            }
        }

        public void Reset(IEnumerable<T> items)
        {
            Should.NotBeNull(items, nameof(items));
            using (Lock())
            {
                ResetInternal(items);
            }
        }

        public void RaiseItemChanged(T item, object? args)
        {
            using (Lock())
            {
                var index = IndexOfInternal(item);
                if (index >= 0)
                    GetComponents<ICollectionChangedListener<T>>().OnChanged(this, item, index, args);
            }
        }

        public ActionToken BatchUpdate()
        {
            if (Interlocked.Increment(ref _batchCount) == 1)
                GetComponents<ICollectionBatchUpdateListener>().OnBeginBatchUpdate(this, BatchUpdateType.Source);
            return ActionToken.FromDelegate((@this, _) => ((SynchronizedObservableCollection<T>)@this!).EndBatchUpdate(), this);
        }

        public void Move(int oldIndex, int newIndex)
        {
            using (Lock())
            {
                MoveInternal(oldIndex, newIndex);
            }
        }

        public ActionToken Lock()
        {
            var lockTaken = false;
            object? locker = null;
            try
            {
                while (true)
                {
                    locker = _lastTakenLocker ?? GetLocker();
                    Monitor.Enter(locker, ref lockTaken);

                    var currentLocker = _lastTakenLocker ?? GetLocker();
                    if (ReferenceEquals(currentLocker, locker))
                    {
                        if (lockTaken)
                        {
                            _lastTakenLocker = locker;
                            ++_lockCount;
                            return ActionToken.FromDelegate((c, l) => ((SynchronizedObservableCollection<T>)c!).Unlock(l!), this, locker);
                        }

                        return default;
                    }

                    if (lockTaken)
                    {
                        lockTaken = false;
                        Monitor.Exit(locker);
                    }
                }
            }
            catch
            {
                if (lockTaken && locker != null)
                    Monitor.Exit(locker);
                throw;
            }
        }

        protected virtual void CopyToInternal(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

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

        private object GetLocker()
        {
            lock (_lockLocker)
            {
                return _locker.SyncRoot;
            }
        }

        private void Unlock(object locker)
        {
            if (--_lockCount == 0)
                _lastTakenLocker = null;
            Monitor.Exit(locker);
        }

        private object? Get(int index) => BoxingExtensions.Box(this[index]);

        private void Set(int index, object? value) => this[index] = (T)value!;

        private void EndBatchUpdate()
        {
            if (Interlocked.Decrement(ref _batchCount) == 0)
                GetComponents<ICollectionBatchUpdateListener>().OnEndBatchUpdate(this, BatchUpdateType.Source);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            Should.NotBeNull(array, nameof(array));
            using (Lock())
            {
                CopyToInternal(array, index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        int IList.Add(object? value)
        {
            using (Lock())
            {
                InsertInternal(GetCountInternal(), (T)value!, true);
                return GetCountInternal() - 1;
            }
        }

        bool IList.Contains(object? value)
        {
            if (IsCompatibleObject(value))
                return Contains((T)value!);
            return false;
        }

        int IList.IndexOf(object? value)
        {
            if (IsCompatibleObject(value!))
                return IndexOf((T)value!);
            return -1;
        }

        void IList.Insert(int index, object? value) => Insert(index, (T)value!);

        void IList.Remove(object? value)
        {
            if (IsCompatibleObject(value!))
                Remove((T)value!);
        }

        void IObservableCollection.Reset(IEnumerable<object> items) => Reset((IEnumerable<T>)items);

        void IObservableCollection.RaiseItemChanged(object item, object? args) => RaiseItemChanged((T)item, args);

        void ISynchronizable.UpdateLocker(ILocker locker)
        {
            Should.NotBeNull(locker, nameof(locker));
            var set = false;
            lock (_lockLocker)
            {
                if (locker.Priority > _locker.Priority)
                {
                    _locker = locker;
                    set = true;
                }
            }

            if (set)
                GetComponents<ILockerChangedListener>().OnChanged(this, locker, null);
        }

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly SynchronizedObservableCollection<T>? _collection;
            private ActionToken _locker;
            private int _index;

            internal Enumerator(SynchronizedObservableCollection<T> collection)
            {
                _collection = collection;
                _index = -1;
                _locker = collection.Lock();
            }

            public T Current => _collection == null ? default! : _collection[_index];

            object IEnumerator.Current => Current!;

            public bool MoveNext() => _collection != null && ++_index < _collection.GetCountInternal();

            public void Reset() => _index = -1;

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

            public IEnumerable<T> Items => _collection.Items;

            public IEnumerable<object?> DecoratedItems => _collection.Decorate().ToArray();
        }
    }
}