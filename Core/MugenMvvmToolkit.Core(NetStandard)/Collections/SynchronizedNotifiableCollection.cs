#region Copyright

// ****************************************************************************
// <copyright file="SynchronizedNotifiableCollection.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Collections
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class SynchronizedNotifiableCollection<T> : INotifiableCollection, INotifiableCollection<T>
#if NET_STANDARD
        , IReadOnlyList<T>
#endif
    {
        #region Nested types

        public struct Enumerator : IEnumerator<T>
        {
            #region Fields

            private readonly SynchronizedNotifiableCollection<T> _collection;
            private int _index;
            private T _current;

            #endregion

            #region Constructors

            public Enumerator(SynchronizedNotifiableCollection<T> collection)
            {
                _collection = collection;
                _index = 0;
                _current = default(T);
            }

            #endregion

            #region Implementatio of IEnumerator<T>

            public bool MoveNext()
            {
                if (_collection == null)
                    return false;
                lock (_collection.Locker)
                {
                    var items = _collection.GetItems();
                    if (_index >= _collection.GetCountInternal(items))
                        return false;
                    _current = _collection.GetItemInternal(items, _index);
                    ++_index;
                    return true;
                }
            }

            public void Reset()
            {
                _index = 0;
            }

            public T Current => _current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            #endregion

        }

        [Flags]
        protected enum NotificationType
        {
            None = 0,
            Changing = 1,
            Changed = 2,
            UnsafeChanged = 4,
            ChangingUnsafe = Changing | UnsafeChanged,
            All = Changing | Changed | UnsafeChanged
        }

        #endregion

        #region Fields

        [XmlIgnore, NonSerialized]
        private object _locker;

        [XmlIgnore, NonSerialized]
        private bool _isNotificationsDirty;

        private IList<T> _items;

        [XmlIgnore, NonSerialized]
        private bool _notifying;

        [XmlIgnore, NonSerialized]
        private bool _hasClearAction;

        [XmlIgnore, NonSerialized]
        private List<Action<SynchronizedNotifiableCollection<T>>> _pendingChanges;

        [XmlIgnore, NonSerialized]
        private IList<T> _snapshot;

        [XmlIgnore, NonSerialized]
        private int _suspendCount;

        [XmlIgnore, NonSerialized]
        private IThreadManager _threadManager;

        [XmlIgnore, NonSerialized]
        internal NotifyCollectionChangedEventHandler AfterCollectionChanged;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly bool IsRefType;

        #endregion

        #region Constructors

        static SynchronizedNotifiableCollection()
        {
            IsRefType = default(T) == null;
        }

        public SynchronizedNotifiableCollection(IList<T> list, IThreadManager threadManager = null)
        {
            Should.NotBeNull(list, nameof(list));
            if (list.IsReadOnly)
                list = new List<T>(list);
            _threadManager = threadManager;
            Items = list;
            BatchSize = ApplicationSettings.NotificationCollectionBatchSize;
        }

        public SynchronizedNotifiableCollection()
            : this(new List<T>())
        {
        }

        public SynchronizedNotifiableCollection(IThreadManager threadManager)
            : this(new List<T>(), threadManager)
        {
        }

        public SynchronizedNotifiableCollection(IEnumerable<T> collection, IThreadManager threadManager = null)
            : this(new List<T>(collection), threadManager)
        {
        }

        #endregion

        #region Properties

        public IThreadManager ThreadManager
        {
            get
            {
                if (_threadManager == null)
                    return ServiceProvider.ThreadManager;
                return _threadManager;
            }
            set { _threadManager = value; }
        }

        [DataMember]
        public int BatchSize { get; set; }

        object ICollection.SyncRoot => Locker;

        bool ICollection.IsSynchronized => true;

        bool IList.IsReadOnly => false;

        bool ICollection<T>.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        public bool IsNotificationsSuspended => _suspendCount != 0;

        public T this[int index]
        {
            get
            {
                lock (Locker)
                    return GetItemInternal(GetItems(), index);
            }
            set
            {
                lock (Locker)
                {
                    if (IsUiThread())
                    {
                        EnsureSynchronized();
                        SetItemInternal(Items, index, value, NotificationType.All);
                    }
                    else
                    {
                        InitializePendingChanges();
                        if (SetItemInternal(Items, index, value, NotificationType.ChangingUnsafe))
                            AddPendingAction(c => SetItemInternal(c._snapshot, index, value, NotificationType.Changed), false);
                    }
                }
            }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        public int Count
        {
            get
            {
                lock (Locker)
                    return GetCountInternal(GetItems());
            }
        }

        [DataMember]
        protected internal IList<T> Items
        {
            get { return _items; }
            set { _items = OnItemsChanged(value); }
        }

        protected object Locker
        {
            get
            {
                if (_locker == null)
                {
                    var collection = Items as ICollection;
                    if (collection != null && collection.IsSynchronized)
                        _locker = collection.SyncRoot;
                    else
                        Interlocked.CompareExchange(ref _locker, new object(), null);
                }
                return _locker;
            }
        }

        #endregion

        #region Methods

        public int Add(T item)
        {
            lock (Locker)
                return AddNoLock(item);
        }

        public int Insert(int index, T item)
        {
            lock (Locker)
            {
                if (IsUiThread())
                {
                    EnsureSynchronized();
                    return InsertItemInternal(Items, index, item, false, NotificationType.All);
                }
                InitializePendingChanges();
                var i = InsertItemInternal(Items, index, item, false, NotificationType.ChangingUnsafe);
                if (i >= 0)
                    AddPendingAction(c => c.InsertItemInternal(c._snapshot, index, item, false, NotificationType.Changed), false);
                return i;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        protected virtual IList<T> OnItemsChanged(IList<T> items)
        {
            //Suppress array usage on deserialization
            if (items is T[])
                return new List<T>(items);
            return items;
        }

        protected virtual IList<T> CreateSnapshotCollection(IList<T> items)
        {
            return new List<T>(items);
        }

        protected virtual T GetItemInternal(IList<T> items, int index)
        {
            return items[index];
        }

        protected virtual int GetCountInternal(IList<T> items)
        {
            return items.Count;
        }

        protected virtual bool SetItemInternal(IList<T> items, int index, T item, NotificationType notificationType)
        {
            var oldItem = items[index];
            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Replace, oldItem, item, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }

            items[index] = item;
            OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index), notificationType);
            return true;
        }

        protected virtual int InsertItemInternal(IList<T> items, int index, T item, bool isAdd, NotificationType notificationType)
        {
            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Add, item, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return -1;
            }

            items.Insert(index, item);
            OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index), notificationType);
            return index;
        }

        protected virtual bool ClearItemsInternal(IList<T> items, NotificationType notificationType)
        {
            if (items.Count == 0)
                return true;
            if (HasChangingFlag(notificationType))
            {
                var args = GetCollectionChangeArgs();
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }
            items.Clear();
            OnCollectionChanged(Empty.ResetEventArgs, notificationType);
            return true;
        }

        protected virtual bool RemoveItemInternal(IList<T> items, int index, NotificationType notificationType)
        {
            var removedItem = items[index];
            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Remove, removedItem, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }
            items.RemoveAt(index);
            OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index), notificationType);
            return true;
        }

        protected virtual bool ContainsInternal(IList<T> items, T item)
        {
            return items.Contains(item);
        }

        protected virtual void CopyToInternal(IList<T> items, Array array, int index)
        {
            var genericArray = array as T[];
            var count = GetCountInternal(items);
            if (genericArray == null)
            {
                for (var i = index; i < count; i++)
                {
                    if (i >= array.Length)
                        break;
                    array.SetValue(GetItemInternal(items, i), i);
                }
            }
            else
            {
                for (var i = index; i < count; i++)
                {
                    if (i >= genericArray.Length)
                        break;
                    genericArray[i] = GetItemInternal(items, i);
                }
            }
        }

        protected virtual int IndexOfInternal(IList<T> items, T item)
        {
            return items.IndexOf(item);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e, NotificationType type)
        {
            bool isUnsafe = HasUnsafeFlag(type);
            bool isChanged = HasChangedFlag(type);
            if (!isChanged && !isUnsafe)
                return;
            if (IsNotificationsSuspended)
                _isNotificationsDirty = true;
            else
            {
                if (isUnsafe)
                    CollectionChangedUnsafe?.Invoke(this, e);
                if (!isChanged)
                    return;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Reset:
                        OnPropertyChanged(Empty.CountChangedArgs);
                        break;
                }
                OnPropertyChanged(Empty.IndexerPropertyChangedArgs);
                if (_snapshot == null)
                    CollectionChanged?.Invoke(this, e);
                else
                {
                    var handler = CollectionChanged;
                    if (handler != null)
                    {
                        var delegates = handler.GetInvocationList();
                        for (int i = 0; i < delegates.Length; i++)
                        {
                            ((NotifyCollectionChangedEventHandler)delegates[i]).Invoke(this, e);
                            if (_snapshot == null)
                                return;
                        }
                    }
                }
                AfterCollectionChanged?.Invoke(this, e);
            }
        }

        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs e)
        {
            CollectionChanging?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void InitializePendingChanges()
        {
            if (_snapshot == null)
                _snapshot = CreateSnapshotCollection(Items);
            if (_pendingChanges == null)
                _pendingChanges = new List<Action<SynchronizedNotifiableCollection<T>>>();
        }

        protected virtual void TryRaisePendingChanges()
        {
            if (_notifying || _pendingChanges == null || _pendingChanges.Count == 0 || IsNotificationsSuspended)
                return;
            try
            {
                _notifying = true;
                for (var i = 0; i < _pendingChanges.Count; i++)
                    _pendingChanges[i].Invoke(this);
            }
            finally
            {
                ClearPendingChanges();
                _notifying = false;
            }
        }

        protected virtual void ClearPendingChanges()
        {
            _snapshot = null;
            _pendingChanges?.Clear();
            _hasClearAction = false;
        }

        protected bool IsUiThread()
        {
            return ThreadManager.IsUiThread;
        }

        protected bool EnsureSynchronized()
        {
            if (_snapshot == null)
                return false;
            RaiseResetInternal();
            return true;
        }

        protected void AddPendingAction(Action<SynchronizedNotifiableCollection<T>> pendingAction, bool isClear)
        {
            if (_hasClearAction)
                return;
            InitializePendingChanges();
            if (isClear || _pendingChanges.Count + 1 >= BatchSize)
            {
                _pendingChanges.Clear();
                _hasClearAction = true;
                pendingAction = c => c.RaiseResetInternal();
            }
            _pendingChanges.Add(pendingAction);
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, this, (collection, _) =>
            {
                lock (collection.Locker)
                    collection.TryRaisePendingChanges();
            });
        }

        private int AddNoLock(T item)
        {
            if (IsUiThread())
            {
                EnsureSynchronized();
                return InsertItemInternal(Items, GetCountInternal(Items), item, true, NotificationType.All);
            }

            InitializePendingChanges();
            var i = InsertItemInternal(Items, GetCountInternal(Items), item, true, NotificationType.ChangingUnsafe);
            if (i >= 0)
                AddPendingAction(c => InsertItemInternal(c._snapshot, c.GetCountInternal(c._snapshot), item, true, NotificationType.Changed), false);
            return i;
        }

        private bool RemoveNoLock(T item)
        {
            int index;
            if (IsUiThread())
            {
                EnsureSynchronized();
                index = IndexOfInternal(Items, item);
                return index >= 0 && RemoveItemInternal(Items, index, NotificationType.All);
            }
            index = IndexOfInternal(Items, item);
            var r = false;
            if (index >= 0)
            {
                InitializePendingChanges();
                r = RemoveItemInternal(Items, index, NotificationType.ChangingUnsafe);
            }
            if (r)
            {
                AddPendingAction(c =>
                {
                    var indexOf = c.IndexOfInternal(c._snapshot, item);
                    if (indexOf >= 0)
                        RemoveItemInternal(c._snapshot, indexOf, NotificationType.Changed);
                }, false);
            }
            return r;
        }

        private void EndSuspendNotifications()
        {
            bool isEnd = false;
            lock (Locker)
            {
                if (_suspendCount == 1)
                    isEnd = true;
                else
                    --_suspendCount;
            }
            if (isEnd)
            {
                ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, this, (@this, _) =>
                {
                    lock (@this.Locker)
                    {
                        --@this._suspendCount;
                        if (@this._suspendCount == 0)
                        {
                            if (@this._isNotificationsDirty)
                            {
                                @this._isNotificationsDirty = false;
                                @this.RaiseResetInternal();
                            }
                            else
                                @this.TryRaisePendingChanges();
                        }
                    }
                    if (@this._suspendCount == 0)
                        @this.OnPropertyChanged(Empty.IsNotificationsSuspendedChangedArgs);
                });
            }
        }

        protected void RaiseResetInternal()
        {
            ClearPendingChanges();
            OnCollectionChanged(Empty.ResetEventArgs, NotificationType.All);
        }

        protected IList<T> GetItems()
        {
            if (IsUiThread() && _snapshot != null)
                return _snapshot;
            return Items;
        }

        protected static bool HasChangingFlag(NotificationType type)
        {
            return (type & NotificationType.Changing) == NotificationType.Changing;
        }

        protected static bool HasChangedFlag(NotificationType type)
        {
            return (type & NotificationType.Changed) == NotificationType.Changed;
        }

        protected static bool HasUnsafeFlag(NotificationType type)
        {
            return (type & NotificationType.UnsafeChanged) == NotificationType.UnsafeChanged;
        }

        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs(NotifyCollectionChangedAction action,
            object item, int index)
        {
            return new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs(NotifyCollectionChangedAction action,
            object oldItem, object newItem, int index)
        {
            return new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs()
        {
            return new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected internal static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;
            return value == null && IsRefType;
        }

        #endregion

        #region Implementation of interfaces

        void ICollection.CopyTo(Array array, int index)
        {
            lock (Locker)
                CopyToInternal(GetItems(), array, index);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            lock (Locker)
            {
                if (IsUiThread())
                {
                    EnsureSynchronized();
                    if (ClearItemsInternal(Items, NotificationType.All))
                        ClearPendingChanges();
                }
                else
                {
                    InitializePendingChanges();
                    if (ClearItemsInternal(Items, NotificationType.ChangingUnsafe))
                        AddPendingAction(c => c.RaiseResetInternal(), true);
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (Locker)
            {
                if (IsUiThread())
                {
                    EnsureSynchronized();
                    RemoveItemInternal(Items, index, NotificationType.All);
                }
                else
                {
                    InitializePendingChanges();
                    if (RemoveItemInternal(Items, index, NotificationType.ChangingUnsafe))
                        AddPendingAction(c => c.RemoveItemInternal(c._snapshot, index, NotificationType.Changed), false);
                }
            }
        }

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        public virtual event NotifyCollectionChangedEventHandler CollectionChangedUnsafe;

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual event NotifyCollectionChangingEventHandler CollectionChanging;

        public virtual IDisposable SuspendNotifications()
        {
            lock (Locker)
                ++_suspendCount;
            return WeakActionToken.Create(this, collection => collection.EndSuspendNotifications());
        }

        public void RaiseReset()
        {
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, this, (@this, _) =>
            {
                lock (@this.Locker)
                    @this.RaiseResetInternal();
            });
        }

        void INotifiableCollection.AddRange(IEnumerable collection, bool suspendNotifications)
        {
            Should.NotBeNull(collection, nameof(collection));
            AddRange(collection.Cast<T>(), suspendNotifications);
        }

        void INotifiableCollection.RemoveRange(IEnumerable collection, bool suspendNotifications)
        {
            Should.NotBeNull(collection, nameof(collection));
            RemoveRange(collection.Cast<T>(), suspendNotifications);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Contains(T item)
        {
            lock (Locker)
                return ContainsInternal(GetItems(), item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (Locker)
                CopyToInternal(GetItems(), array, arrayIndex);
        }

        public bool Remove(T item)
        {
            lock (Locker)
                return RemoveNoLock(item);
        }

        public int IndexOf(T item)
        {
            lock (Locker)
                return IndexOfInternal(GetItems(), item);
        }

        void IList<T>.Insert(int index, T item)
        {
            Insert(index, item);
        }

        public void AddRange(IEnumerable<T> collection, bool suspendNotifications = true)
        {
            bool lockTaken = false;
            IDisposable suspender = null;
            try
            {
                if (suspendNotifications)
                    suspender = SuspendNotifications();
                Monitor.Enter(Locker, ref lockTaken);
                foreach (var item in collection)
                    AddNoLock(item);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(Locker);
                suspender?.Dispose();
            }
        }

        public void RemoveRange(IEnumerable<T> collection, bool suspendNotifications = true)
        {
            bool lockTaken = false;
            IDisposable suspender = null;
            try
            {
                if (suspendNotifications)
                    suspender = SuspendNotifications();
                Monitor.Enter(Locker, ref lockTaken);
                foreach (var item in collection)
                    RemoveNoLock(item);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(Locker);
                suspender?.Dispose();
            }
        }

        public void Update(IEnumerable<T> items)
        {
            Should.NotBeNull(items, nameof(items));
            using (SuspendNotifications())
            {
                Clear();
                AddRange(items);
            }
        }

        public T[] ToArray()
        {
            lock (Locker)
            {
                var items = GetItems();
                var count = GetCountInternal(items);
                if (count == 0)
                    return Empty.Array<T>();
                var result = new T[count];
                for (int i = 0; i < count; i++)
                    result[i] = GetItemInternal(items, i);
                return result;
            }
        }

        #endregion
    }
}