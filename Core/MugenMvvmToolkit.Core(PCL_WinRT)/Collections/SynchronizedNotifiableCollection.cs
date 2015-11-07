#region Copyright

// ****************************************************************************
// <copyright file="SynchronizedNotifiableCollection.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Collections.ObjectModel;
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
    [DebuggerDisplay("Count = {Count}, NotificationCount = {NotificationCount}")]
    public class SynchronizedNotifiableCollection<T> : INotifiableCollection, INotifiableCollection<T>
    {
        #region Nested types

        [DebuggerDisplay("AddedCount = {AddedCount}, RemovedCount = {RemovedCount}")]
        protected internal sealed class EventTracker
        {
            #region Fields

            private readonly SynchronizedNotifiableCollection<T> _collection;
            private readonly Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventArgs>> _events;
            private bool _hasResetEvent;

            #endregion

            #region Constructors

            public EventTracker(SynchronizedNotifiableCollection<T> collection)
            {
                _collection = collection;
                _events = new Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventArgs>>();
            }

            #endregion

            #region Properties

            public int AddedCount
            {
                get
                {
                    List<NotifyCollectionChangedEventArgs> list;
                    _events.TryGetValue(NotifyCollectionChangedAction.Add, out list);
                    if (list == null)
                        return 0;
                    return list.Count;
                }
            }

            public int RemovedCount
            {
                get
                {
                    List<NotifyCollectionChangedEventArgs> list;
                    _events.TryGetValue(NotifyCollectionChangedAction.Remove, out list);
                    if (list == null)
                        return 0;
                    return list.Count;
                }
            }

            public Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventArgs>>.ValueCollection Events
            {
                get { return _events.Values; }
            }

            private int Count
            {
                get
                {
                    int count = 0;
                    foreach (var @event in _events)
                        count += @event.Value.Count;
                    return count;
                }
            }

            #endregion

            #region Methods

            public void AddEvent(NotifyCollectionChangedEventArgs args)
            {
                if (_hasResetEvent)
                    return;
                if (Count >= _collection.BatchSize)
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                bool shouldIgnore = false;
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        OnAddEvent(ref args);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        OnRemoveEvent(ref args, ref shouldIgnore);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        OnReplaceEvent(ref args, ref shouldIgnore);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _hasResetEvent = true;
                        _events.Clear();
                        break;
                }

                if (shouldIgnore) return;
                List<NotifyCollectionChangedEventArgs> list;
                if (!_events.TryGetValue(args.Action, out list))
                {
                    list = new List<NotifyCollectionChangedEventArgs>();
                    _events[args.Action] = list;
                }
                list.Add(args);
            }

            public void Clear()
            {
                _events.Clear();
                _hasResetEvent = false;
            }

            private void OnAddEvent(ref NotifyCollectionChangedEventArgs args)
            {
                List<NotifyCollectionChangedEventArgs> list;
                if (_events.TryGetValue(NotifyCollectionChangedAction.Add, out list) && list.Count != 0)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        NotifyCollectionChangedEventArgs oldArgs = list[index];
                        if (oldArgs.NewStartingIndex < args.NewStartingIndex) continue;
                        list[index] = UpdateAddEvent(oldArgs.NewItems, oldArgs.NewStartingIndex + 1);
                    }
                }

                if (_events.TryGetValue(NotifyCollectionChangedAction.Replace, out list) && list.Count != 0)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        NotifyCollectionChangedEventArgs oldArgs = list[index];
                        if (oldArgs.NewStartingIndex < args.NewStartingIndex) continue;
                        list[index] = UpdateReplaceEvent(oldArgs.NewItems, oldArgs.OldItems,
                            oldArgs.NewStartingIndex + 1);
                    }
                }
            }

            private void OnRemoveEvent(ref NotifyCollectionChangedEventArgs args, ref bool shouldIgnore)
            {
                List<NotifyCollectionChangedEventArgs> list;
                if (_events.TryGetValue(NotifyCollectionChangedAction.Add, out list) && list.Count != 0)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        NotifyCollectionChangedEventArgs oldArgs = list[index];
                        if (oldArgs.NewStartingIndex == args.OldStartingIndex &&
                            SequenceEqual(oldArgs.NewItems, args.OldItems))
                        {
                            list.RemoveAt(index);
                            index--;
                            shouldIgnore = true;
                            continue;
                        }
                        if (oldArgs.NewStartingIndex < args.OldStartingIndex) continue;
                        int newIndex = oldArgs.NewStartingIndex - 1;
                        if (newIndex < 0)
                        {
                            list.RemoveAt(index);
                            index--;
                            shouldIgnore = true;
                        }
                        else
                            list[index] = UpdateAddEvent(oldArgs.NewItems, newIndex);
                    }
                }

                if (_events.TryGetValue(NotifyCollectionChangedAction.Replace, out list) && list.Count != 0)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        NotifyCollectionChangedEventArgs oldArgs = list[index];
                        if (oldArgs.NewStartingIndex < args.OldStartingIndex) continue;
                        int replaceIndex = oldArgs.NewStartingIndex - 1;
                        if (replaceIndex < 0)
                        {
                            list.RemoveAt(index);
                            index--;
                        }
                        else
                            list[index] = UpdateReplaceEvent(oldArgs.NewItems, oldArgs.OldItems, replaceIndex);
                    }
                }
            }

            private void OnReplaceEvent(ref NotifyCollectionChangedEventArgs args, ref bool shouldIgnore)
            {
                List<NotifyCollectionChangedEventArgs> list;
                if (_events.TryGetValue(NotifyCollectionChangedAction.Add, out list) && list.Count != 0)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        if (list[index].NewStartingIndex == args.NewStartingIndex)
                        {
                            list[index] = UpdateAddEvent(args.NewItems, args.NewStartingIndex);
                            shouldIgnore = true;
                            break;
                        }
                    }
                }
            }

            private static NotifyCollectionChangedEventArgs UpdateAddEvent(IList items, int index)
            {
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items[0], index);
            }

            private static NotifyCollectionChangedEventArgs UpdateReplaceEvent(IList newItems, IList oldItems, int index)
            {
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems[0],
                    oldItems[0], index);
            }

            private static bool SequenceEqual(IEnumerable enumerable1, IEnumerable enumerable2)
            {
                return enumerable1.OfType<object>().SequenceEqual(enumerable2.OfType<object>());
            }

            #endregion
        }

        #endregion

        #region Fields

        [DataMember]
        protected internal readonly object Locker;

        [DataMember]
        internal IList<T> ItemsInternal;

        [XmlIgnore, NonSerialized]
        private EventTracker _eventsTracker;
        [XmlIgnore, NonSerialized]
        private int _notificationCount;

        [XmlIgnore, NonSerialized]
        private int _suspendCount;
        [XmlIgnore, NonSerialized]
        private IThreadManager _threadManager;

        [XmlIgnore, NonSerialized]
        private Action _raiseEventsDelegate;

        internal NotifyCollectionChangedEventHandler BeforeCollectionChanged;

        internal NotifyCollectionChangedEventHandler AfterCollectionChanged;

        #endregion

        #region Constructors

        public SynchronizedNotifiableCollection(IList<T> list, IThreadManager threadManager = null)
        {
            if (list == null)
                list = new List<T>();
            else
            {
                if (list.IsReadOnly)
                    list = new List<T>(list);
            }
            _raiseEventsDelegate = RaiseEventsInternal;
            _eventsTracker = new EventTracker(this);
            _threadManager = threadManager;

            Items = list;
            var collection = Items as ICollection;
            Locker = collection == null ? new object() : collection.SyncRoot;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            OnInitialized();
        }

        public SynchronizedNotifiableCollection()
            : this(new List<T>(), null)
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

        public int NotificationCount
        {
            get { return _notificationCount; }
        }

        public int Count
        {
            get
            {
                lock (Locker)
                    return CountInternal;
            }
        }

        public int BatchSize { get; set; }

        public virtual ExecutionMode ExecutionMode { get; set; }

        public virtual NotificationCollectionMode NotificationMode { get; set; }

        protected virtual int CountInternal
        {
            get { return Items.Count; }
        }

        protected internal IList<T> Items
        {
            get
            {
                CheckDeserialization();
                return ItemsInternal;
            }
            set { ItemsInternal = value; }
        }

        protected internal EventTracker EventsTracker
        {
            get
            {
                CheckDeserialization();
                return _eventsTracker;
            }
        }

        #endregion

        #region Methods

        public void InvokeWithLock(Action<SynchronizedNotifiableCollection<T>> action)
        {
            lock (Locker)
            {
                using (SuspendNotifications())
                    action(this);
            }
        }

        public void RaiseReset()
        {
            lock (Locker)
                EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RaiseEvents();
        }

        public void Update(IEnumerable<T> items)
        {
            Should.NotBeNull(items, "items");
            if (Items == null)
                return;
            using (SuspendNotifications())
            {
                Clear();
                AddRange(items);
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                foreach (T item in collection)
                    Add(item);
            }
        }

        void INotifiableCollection.AddRange(IEnumerable collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                foreach (object item in collection)
                    Add((T)item);
            }
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                foreach (var item in collection)
                    Remove(item);
            }
        }

        void INotifiableCollection.RemoveRange(IEnumerable collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                foreach (T item in collection)
                    Remove(item);
            }
        }

        public bool Replace(T oldValue, T newValue)
        {
            bool shouldRaiseEvents;
            lock (Locker)
            {
                int index = IndexOfInternal(oldValue);
                if (index == -1)
                    return false;
                SetItemInternal(index, newValue, out shouldRaiseEvents);
            }
            if (shouldRaiseEvents)
                RaiseEvents();
            return true;
        }

        protected virtual void OnInitialized()
        {
            ExecutionMode = ApplicationSettings.SynchronizedCollectionExecutionMode;
            NotificationMode = ApplicationSettings.NotificationCollectionMode;
            _notificationCount = CountInternal;
            BatchSize = ApplicationSettings.NotificationCollectionBatchSize;
        }

        protected virtual void CopyToInternal(Array array, int index)
        {
            int count = Items.Count;
            for (int i = index; i < count; i++)
                array.SetValue(Items[i], i);
        }

        protected virtual void ClearItemsInternal(out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            if (Items.Count == 0)
                return;
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs();
            OnCollectionChanging(args);
            if (args.Cancel)
                return;

            Items.Clear();
            EventsTracker.AddEvent(args.ChangedEventArgs);
            shouldRaiseEvents = true;
        }

        protected virtual void RemoveItemInternal(int index, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            T removedItem = Items[index];
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Remove,
                removedItem, index);
            OnCollectionChanging(args);
            if (args.Cancel)
                return;

            Items.RemoveAt(index);
            EventsTracker.AddEvent(args.ChangedEventArgs);
            shouldRaiseEvents = true;
        }

        protected virtual void SetItemInternal(int index, T item, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            T oldItem = Items[index];
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Replace,
                oldItem, item,
                index);
            OnCollectionChanging(args);
            if (args.Cancel) return;

            Items[index] = item;
            EventsTracker.AddEvent(args.ChangedEventArgs);
            shouldRaiseEvents = true;
        }

        protected virtual T GetItemInternal(int index)
        {
            return Items[index];
        }

        protected virtual int InsertItemInternal(int index, T item, bool isAdd, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Add, item,
                index);
            OnCollectionChanging(args);
            if (args.Cancel)
                return -1;

            Items.Insert(index, item);
            EventsTracker.AddEvent(args.ChangedEventArgs);
            shouldRaiseEvents = true;
            return index;
        }

        protected virtual int IndexOfInternal(T item)
        {
            return Items.IndexOf(item);
        }

        protected virtual bool ContainsInternal(T item)
        {
            return Items.Contains(item);
        }

        protected virtual IEnumerator<T> GetEnumeratorInternal()
        {
            return Items.GetEnumerator();
        }

        protected virtual void RaiseEvents()
        {
            if (!IsNotificationsSuspended)
                ThreadManager.Invoke(ExecutionMode, _raiseEventsDelegate);
        }

        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs e)
        {
            NotifyCollectionChangingEventHandler handler = CollectionChanging;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs(NotifyCollectionChangedAction action,
            object item, int index)
        {
            return new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs(NotifyCollectionChangedAction action,
            object oldItem, object newItem,
            int index)
        {
            return
                new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(action, newItem, oldItem,
                    index));
        }

        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs()
        {
            return
                new NotifyCollectionChangingEventArgs(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected internal static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;
            if (value == null)
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                return default(T) == null;
            return false;
        }

        private void Notify(NotifyCollectionChangedEventArgs args, int count)
        {
            if (BeforeCollectionChanged != null)
                BeforeCollectionChanged(this, args);
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _notificationCount++;
                    OnPropertyChanged(Empty.CountChangedArgs);
                    OnPropertyChanged(Empty.NotificationCountChangedArgs);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _notificationCount--;
                    OnPropertyChanged(Empty.CountChangedArgs);
                    OnPropertyChanged(Empty.NotificationCountChangedArgs);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _notificationCount = count;
                    OnPropertyChanged(Empty.CountChangedArgs);
                    OnPropertyChanged(Empty.NotificationCountChangedArgs);
                    break;
            }
            OnPropertyChanged(Empty.IndexerPropertyChangedArgs);
            OnCollectionChanged(args);
            if (AfterCollectionChanged != null)
                AfterCollectionChanged(this, args);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) != 0)
                return;
            RaiseEvents();
            OnPropertyChanged(Empty.IsNotificationsSuspendedChangedArgs);
        }

        private void CheckDeserialization()
        {
            if (_eventsTracker != null)
                return;
            _eventsTracker = new EventTracker(this);
            if (Items is T[])
                Items = new ObservableCollection<T>(Items);
            if (_raiseEventsDelegate == null)
                _raiseEventsDelegate = RaiseEventsInternal;
            OnInitialized();
        }

        private void RaiseEventsInternal()
        {
            lock (Locker)
            {
                try
                {
                    int count = CountInternal;
                    List<NotifyCollectionChangedEventArgs> sortedItems = null;
                    _notificationCount = count - EventsTracker.AddedCount + EventsTracker.RemovedCount;

                    foreach (var events in EventsTracker.Events)
                    {
                        for (int i = 0; i < events.Count; i++)
                        {
                            var args = events[i];
                            if (args.OldStartingIndex != -1)
                            {
                                if (args.OldStartingIndex > _notificationCount)
                                {
                                    if (sortedItems == null)
                                        sortedItems = new List<NotifyCollectionChangedEventArgs>();
                                    sortedItems.Add(args);
                                    continue;
                                }
                            }

                            if (args.NewStartingIndex != -1)
                            {
                                if (args.NewStartingIndex > _notificationCount)
                                {
                                    if (sortedItems == null)
                                        sortedItems = new List<NotifyCollectionChangedEventArgs>();
                                    sortedItems.Add(args);
                                    continue;
                                }
                            }
                            Notify(args, count);
                        }
                    }
                    if (sortedItems == null)
                        return;
                    sortedItems.Sort((args, eventArgs) =>
                    {
                        int x1 = args.OldStartingIndex != -1 ? args.OldStartingIndex : args.NewStartingIndex;
                        int x2 = eventArgs.OldStartingIndex != -1
                            ? eventArgs.OldStartingIndex
                            : eventArgs.NewStartingIndex;
                        return x1.CompareTo(x2);
                    });
                    for (int index = 0; index < sortedItems.Count; index++)
                        Notify(sortedItems[index], count);
                }
                finally
                {
                    EventsTracker.Clear();
                }
            }
        }

        private bool UseNotificationMode(bool generic)
        {
            var flag = generic
                ? NotificationCollectionMode.GenericCollectionInterfaceUseNotificationValue
                : NotificationCollectionMode.CollectionIntefaceUseNotificationValue;
            if (!NotificationMode.HasFlagEx(flag))
                return false;
            return !NotificationMode.HasFlagEx(NotificationCollectionMode.OnlyOnUiThread) || ThreadManager.IsUiThread;
        }

        #endregion

        #region Implementation of ISuspendNotifications

        public virtual bool IsNotificationsSuspended
        {
            get { return _suspendCount != 0; }
        }

        public virtual IDisposable SuspendNotifications()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
                OnPropertyChanged(Empty.IsNotificationsSuspendedChangedArgs);
            return WeakActionToken.Create(this, collection => collection.EndSuspendNotifications());
        }

        #endregion

        #region Implementation of notification events

        [field: XmlIgnore, NonSerialized]
        public virtual event NotifyCollectionChangingEventHandler CollectionChanging;

        [field: XmlIgnore, NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        [field: XmlIgnore, NonSerialized]
        public virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (Locker)
                return GetEnumeratorInternal();
        }

        #endregion

        #region Implementation of ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            lock (Locker)
                CopyToInternal(array, index);
        }

        int ICollection.Count
        {
            get
            {
                if (UseNotificationMode(false))
                    return NotificationCount;
                return Count;
            }
        }

        object ICollection.SyncRoot
        {
            get { return Locker; }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        #endregion

        #region Implementation of IList

        int IList.Add(object value)
        {
            bool shouldRaiseEvents;
            int count;
            lock (Locker)
                count = InsertItemInternal(CountInternal, (T)value, true, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
            return count;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        void IList.Clear()
        {
            Clear();
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

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                if (UseNotificationMode(false))
                {
                    lock (Locker)
                    {
                        if (index >= CountInternal)
                            return null;
                        return GetItemInternal(index);
                    }
                }
                return this[index];
            }
            set { this[index] = (T)value; }
        }

        bool IList.IsReadOnly
        {
            get { return IsReadOnly; }
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        #endregion

        #region Implementation of ICollection<T>

        public void Add(T item)
        {
            bool shouldRaiseEvents;
            lock (Locker)
                InsertItemInternal(CountInternal, item, true, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        public void Clear()
        {
            bool shouldRaiseEvents;
            lock (Locker)
                ClearItemsInternal(out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        public bool Contains(T item)
        {
            lock (Locker)
                return ContainsInternal(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (Locker)
                CopyToInternal(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            bool shouldRaiseEvents;
            lock (Locker)
            {
                int index = IndexOfInternal(item);
                if (index < 0)
                    return false;
                RemoveItemInternal(index, out shouldRaiseEvents);
            }
            if (shouldRaiseEvents)
                RaiseEvents();
            return true;
        }

        int ICollection<T>.Count
        {
            get
            {
                if (UseNotificationMode(true))
                    return NotificationCount;
                return Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            lock (Locker)
                return IndexOfInternal(item);
        }

        public void Insert(int index, T item)
        {
            bool shouldRaiseEvents;
            lock (Locker)
                InsertItemInternal(index, item, false, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        public void RemoveAt(int index)
        {
            bool shouldRaiseEvents;
            lock (Locker)
                RemoveItemInternal(index, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        public T this[int index]
        {
            get
            {
                lock (Locker)
                {
                    if (UseNotificationMode(true))
                    {

                        if (index >= CountInternal)
                            return default(T);
                        return GetItemInternal(index);

                    }
                    return GetItemInternal(index);
                }
            }
            set
            {
                bool shouldRaiseEvents;
                lock (Locker)
                    SetItemInternal(index, value, out shouldRaiseEvents);
                if (shouldRaiseEvents)
                    RaiseEvents();
            }
        }

        #endregion
    }
}
