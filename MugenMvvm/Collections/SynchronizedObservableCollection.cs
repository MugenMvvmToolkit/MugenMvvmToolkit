using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(SynchronizedObservableCollection<>.DebuggerProxy))]
    public class SynchronizedObservableCollection<T> : ComponentOwnerBase<ICollection>, IObservableCollection<T>, IObservableCollection, ISynchronizable, ActionToken.IHandler
    {
        private readonly object _locker;
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
            _locker = new object();
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

        object ICollection.SyncRoot => _locker;

        bool IList.IsFixedSize => false;

        Type IReadOnlyObservableCollection.ItemType => typeof(T);

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

        private static bool Equals(ItemOrArray<ISynchronizationListener> x1, ItemOrArray<ISynchronizationListener> x2)
        {
            if (x1.Count != x2.Count)
                return false;

            for (var i = 0; i < x1.Count; i++)
            {
                if (!ReferenceEquals(x1[i], x2[i]))
                    return false;
            }

            return true;
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
            using (Lock())
            {
                if (++_batchCount == 1)
                    GetComponents<ICollectionBatchUpdateListener>().OnBeginBatchUpdate(this, BatchUpdateType.Source);
            }

            return ActionToken.FromDelegate((@this, _) => ((SynchronizedObservableCollection<T>) @this!).EndBatchUpdate(), this);
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
            try
            {
                while (true)
                {
                    var listeners = GetComponents<ISynchronizationListener>();
                    listeners.OnLocking(this, null);
                    Monitor.Enter(_locker, ref lockTaken);
                    listeners.OnLocked(this, null);
                    var lockedListeners = GetComponents<ISynchronizationListener>();
                    if (Equals(listeners, lockedListeners))
                        return ActionToken.FromHandler(this, lockTaken ? _locker : null, listeners.GetRawValue());

                    Unlock(listeners, ref lockTaken);
                }
            }
            catch
            {
                if (lockTaken)
                    Monitor.Exit(_locker);
                throw;
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

        private object? Get(int index) => BoxingExtensions.Box(this[index]);

        private void Set(int index, object? value) => this[index] = (T) value!;

        private void EndBatchUpdate()
        {
            using (Lock())
            {
                if (--_batchCount == 0)
                    GetComponents<ICollectionBatchUpdateListener>().OnEndBatchUpdate(this, BatchUpdateType.Source);
            }
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

        void ActionToken.IHandler.Invoke(object? state1, object? state2)
        {
            bool lockTaken = state1 != null;
            Unlock(ItemOrArray.FromRawValue<ISynchronizationListener>(state2), ref lockTaken);
        }

        private void Unlock(ItemOrArray<ISynchronizationListener> listeners, ref bool lockTaken)
        {
            listeners.OnUnlocking(this, null);
            if (lockTaken)
            {
                Monitor.Exit(_locker);
                lockTaken = false;
            }

            listeners.OnUnlocked(this, null);
        }

        int IList.Add(object? value)
        {
            using (Lock())
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