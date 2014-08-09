#region Copyright
// ****************************************************************************
// <copyright file="SynchronizedNotifiableCollection.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Collections
{
    /// <summary>
    ///     Represents the syncronized observable collection.
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    [DebuggerDisplay("Count = {Count}, NotificationCount = {NotificationCount}")]
    public class SynchronizedNotifiableCollection<T> : INotifiableCollection, INotifiableCollection<T>
    {
        #region Nested types

        /// <summary>
        ///     Represents the class that stores information about notification events.
        /// </summary>
        [DebuggerDisplay("AddedCount = {AddedCount}, RemovedCount = {RemovedCount}")]
        protected internal sealed class EventTracker
        {
            #region Fields

            private readonly SynchronizedNotifiableCollection<T> _collection;
            private readonly Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventArgs>> _events;
            private bool _hasResetEvent;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="EventTracker" /> class.
            /// </summary>
            public EventTracker(SynchronizedNotifiableCollection<T> collection)
            {
                _collection = collection;
                _events = new Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventArgs>>();
            }

            #endregion

            #region Properties

            /// <summary>
            ///     Gets the count of added items.
            /// </summary>
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

            /// <summary>
            ///     Gets the count of removed items.
            /// </summary>
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

            public ICollection<List<NotifyCollectionChangedEventArgs>> Events
            {
                get { return _events.Values; }
            }

            #endregion

            #region Methods

            /// <summary>
            ///     Adds event to collection.
            /// </summary>
            public void AddEvent(NotifyCollectionChangedEventArgs args)
            {
                if (_hasResetEvent)
                    return;
                if (_events.Count >= _collection.BatchSize)
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

            /// <summary>
            ///     Clears events.
            /// </summary>
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
                            continue;
                        }
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
                            continue;
                        }
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

        /// <summary>
        ///     Gets the internal object to synchronize access to collection.
        /// </summary>
        [DataMember]
        protected internal readonly object Locker;

        /// <summary>
        ///     Gets the internal collection.
        /// </summary>
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

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronizedNotifiableCollection{T}" /> class.
        /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronizedNotifiableCollection{T}" /> class.
        /// </summary>
        public SynchronizedNotifiableCollection()
            : this(new List<T>(), null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronizedNotifiableCollection{T}" /> class.
        /// </summary>
        public SynchronizedNotifiableCollection(IThreadManager threadManager)
            : this(new List<T>(), threadManager)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SynchronizedNotifiableCollection{T}" /> class that contains elements
        ///     copied from the specified collection.
        /// </summary>
        /// <param name="threadManager">
        ///     The specified <see cref="IThreadManager" />.
        /// </param>
        /// <param name="collection">
        ///     The collection from which the elements are copied.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     The <paramref name="collection" /> parameter cannot be null.
        /// </exception>
        public SynchronizedNotifiableCollection(IEnumerable<T> collection, IThreadManager threadManager = null)
            : this(new List<T>(collection), threadManager)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the <see cref="IThreadManager" />
        /// </summary>
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

        /// <summary>
        ///     Gets the number of notification elements contained in the collection.
        /// </summary>
        public int NotificationCount
        {
            get { return _notificationCount; }
        }

        /// <summary>
        ///     Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                lock (Locker)
                    return CountInternal;
            }
        }

        /// <summary>
        ///     Gets or sets the size that collection will used to notify about changes.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        ///     Specifies the execution mode.
        /// </summary>
        public virtual ExecutionMode ExecutionMode { get; set; }

        /// <summary>
        ///     Gets or sets the count mode.
        /// </summary>
        public virtual NotificationCollectionMode NotificationMode { get; set; }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
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

        /// <summary>
        ///     Gets the event tracker.
        /// </summary>
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

        /// <summary>
        /// Allows to perform an action in the exclusive access to the collection.
        /// </summary>
        /// <param name="action">The specified action to invoke.</param>
        public void InvokeWithLock(Action<SynchronizedNotifiableCollection<T>> action)
        {
            lock (Locker)
            {
                using (SuspendNotifications())
                    action(this);
            }
        }

        /// <summary>
        /// Raises a <see cref="CollectionChanged"/> event of type reset.
        /// </summary>
        public void RaiseReset()
        {
            lock (Locker)
                EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RaiseEvents();
        }

        /// <summary>
        ///     Clears collection and then adds a range of IEnumerable collection.
        /// </summary>
        /// <param name="items">Items to add</param>
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

        /// <summary>
        ///     Adds the specified items to the collection without causing a change notification for all items.
        ///     <para />
        ///     This method will raise a change notification at the end.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="collection" /> is <c>null</c>.
        /// </exception>
        public void AddRange(IEnumerable<T> collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                foreach (T item in collection)
                    Add(item);
            }
        }

        /// <summary>
        ///     Adds the specified items to the collection without causing a change notification for all items.
        ///     <para />
        ///     This method will raise a change notification at the end.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="collection" /> is <c>null</c>.
        /// </exception>
        public void AddRange(IEnumerable collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                foreach (object item in collection)
                    Add((T)item);
            }
        }

        /// <summary>
        ///     Removes the specified items from the collection without causing a change notification for all items.
        ///     <para />
        ///     This method will raise a change notification at the end.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="collection" /> is <c>null</c>.
        /// </exception>
        public void RemoveRange(IEnumerable<T> collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                var list = collection.ToList();
                for (int index = 0; index < list.Count; index++)
                    Remove(list[index]);
            }
        }

        /// <summary>
        ///     Removes the specified items from the collection without causing a change notification for all items.
        ///     <para />
        ///     This method will raise a change notification at the end.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="collection" /> is <c>null</c>.
        /// </exception>
        public void RemoveRange(IEnumerable collection)
        {
            Should.NotBeNull(collection, "collection");
            using (SuspendNotifications())
            {
                var list = collection.Cast<T>().ToList();
                for (int index = 0; index < list.Count; index++)
                    Remove(list[index]);
            }
        }

        /// <summary>
        ///     Replaces the specified item to new item.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
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

        /// <summary>
        ///     Initializes default values.
        /// </summary>
        protected virtual void OnInitialized()
        {
            ExecutionMode = ApplicationSettings.SynchronizedCollectionExecutionMode;
            NotificationMode = ApplicationSettings.NotificationCollectionMode;
            _notificationCount = CountInternal;
            BatchSize = 50;
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see
        ///         cref="T:System.Array" />
        ///     , starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        protected virtual void CopyToInternal(Array array, int index)
        {
            int count = Items.Count;
            for (int i = index; i < count; i++)
                array.SetValue(Items[i], i);
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected virtual void ClearItemsInternal(out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs();
            OnCollectionChanging(args);
            if (args.Cancel)
                return;

            Items.Clear();
            EventsTracker.AddEvent(args.ChangedEventArgs);
            shouldRaiseEvents = true;
        }

        /// <summary>
        ///     Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the element to remove.
        /// </param>
        /// <param name="shouldRaiseEvents"></param>
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

        /// <summary>
        ///     Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the element to replace.
        /// </param>
        /// <param name="item">
        ///     The new value for the element at the specified index.
        /// </param>
        /// <param name="shouldRaiseEvents"></param>
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

        /// <summary>
        ///     Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <returns>An instance of T.</returns>
        protected virtual T GetItemInternal(int index)
        {
            return Items[index];
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index at which <paramref name="item" /> should be inserted.
        /// </param>
        /// <param name="item">
        ///     The object to insert.
        /// </param>
        /// <param name="isAdd"></param>
        /// <param name="shouldRaiseEvents"></param>
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

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <returns>
        ///     The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">
        ///     The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </param>
        protected virtual int IndexOfInternal(T item)
        {
            return Items.IndexOf(item);
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="item">
        ///     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </param>
        protected virtual bool ContainsInternal(T item)
        {
            return Items.Contains(item);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        protected virtual IEnumerator<T> GetEnumeratorInternal()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        ///     Raises events from queue of events.
        /// </summary>
        protected virtual void RaiseEvents()
        {
            if (!IsNotificationsSuspended)
                ThreadManager.Invoke(ExecutionMode, _raiseEventsDelegate);
        }

        /// <summary>
        ///     Invokes the <c>CollectionChanging</c> event.
        /// </summary>
        protected virtual void OnCollectionChanging(NotifyCollectionChangingEventArgs e)
        {
            NotifyCollectionChangingEventHandler handler = CollectionChanging;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        ///     Invokes the <c>CollectionChanged</c> event.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        ///     Invokes the <c>PropertyChanged</c> event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event to any listeners.
        /// </summary>
        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs(NotifyCollectionChangedAction action,
            object item, int index)
        {
            return new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event to any listeners
        /// </summary>
        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs(NotifyCollectionChangedAction action,
            object oldItem, object newItem,
            int index)
        {
            return
                new NotifyCollectionChangingEventArgs(new NotifyCollectionChangedEventArgs(action, newItem, oldItem,
                    index));
        }

        /// <summary>
        ///     Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        protected static NotifyCollectionChangingEventArgs GetCollectionChangeArgs()
        {
            return
                new NotifyCollectionChangingEventArgs(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        ///     Checks that an object is compatible with collection type.
        /// </summary>
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
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _notificationCount++;
                    OnPropertyChanged(MvvmUtilsInternal.CountPropertyChangedArgs);
                    OnPropertyChanged(MvvmUtilsInternal.NotificationCountPropertyChangedArgs);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _notificationCount--;
                    OnPropertyChanged(MvvmUtilsInternal.CountPropertyChangedArgs);
                    OnPropertyChanged(MvvmUtilsInternal.NotificationCountPropertyChangedArgs);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _notificationCount = count;
                    OnPropertyChanged(MvvmUtilsInternal.CountPropertyChangedArgs);
                    OnPropertyChanged(MvvmUtilsInternal.NotificationCountPropertyChangedArgs);
                    break;
            }
            OnPropertyChanged(MvvmUtilsInternal.IndexerPropertyChangedArgs);
            OnCollectionChanged(args);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) != 0)
                return;
            RaiseEvents();
            OnPropertyChanged(new PropertyChangedEventArgs("IsNotificationsSuspended"));
        }

        /// <summary>
        /// OnDeserializedAttribute doesn't work with custom SerializableAttribute 
        /// Exception: Type '' in assembly '' has method 'Test' with an incorrect signature for the serialization attribute that it is decorated with
        /// </summary>
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

        /// <summary>
        ///     Gets or sets a value indicating whether change notifications are suspended. <c>True</c> if notifications are
        ///     suspended, otherwise, <c>false</c>.
        /// </summary>
        public virtual bool IsNotificationsSuspended
        {
            get { return _suspendCount != 0; }
        }

        /// <summary>
        ///     Suspends the change notifications until the returned <see cref="IDisposable" /> is disposed.
        /// </summary>
        /// <returns>An instance of token.</returns>
        public virtual IDisposable SuspendNotifications()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
                OnPropertyChanged(new PropertyChangedEventArgs("IsNotificationsSuspended"));
            return WeakActionToken.Create(this, collection => collection.EndSuspendNotifications(), false);
        }

        #endregion

        #region Implementation of notification events

        /// <summary>
        ///     Occurs before the collection changes.
        /// </summary>
        [field: XmlIgnore, NonSerialized]
        public virtual event NotifyCollectionChangingEventHandler CollectionChanging;

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        [field: XmlIgnore, NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        [field: XmlIgnore, NonSerialized]
        public virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            lock (Locker)
                return GetEnumeratorInternal();
        }

        #endregion

        #region Implementation of ICollection

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />,
        ///     starting at a particular
        ///     <see
        ///         cref="T:System.Array" />
        ///     index.
        /// </summary>
        void ICollection.CopyTo(Array array, int index)
        {
            lock (Locker)
                CopyToInternal(array, index);
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        int ICollection.Count
        {
            get
            {
                if (UseNotificationMode(false))
                    return NotificationCount;
                return Count;
            }
        }

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        object ICollection.SyncRoot
        {
            get { return Locker; }
        }

        /// <summary>
        ///     Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized
        ///     (thread safe).
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        #endregion

        #region Implementation of IList

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.IList" />.
        /// </summary>
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

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.
        /// </summary>
        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
                return Contains((T)value);
            return false;
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.IList" />.
        /// </summary>
        void IList.Clear()
        {
            Clear();
        }

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.
        /// </summary>
        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
                return IndexOf((T)value);
            return -1;
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.
        /// </summary>
        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.
        /// </summary>
        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
                Remove((T)value);
        }

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.IList" /> item at the specified index.
        /// </summary>
        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
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

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.IList" /> is read-only.
        /// </summary>
        bool IList.IsReadOnly
        {
            get { return IsReadOnly; }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </summary>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        #endregion

        #region Implementation of ICollection<T>

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Add(T item)
        {
            bool shouldRaiseEvents;
            lock (Locker)
                InsertItemInternal(CountInternal, item, true, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            bool shouldRaiseEvents;
            lock (Locker)
                ClearItemsInternal(out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        public bool Contains(T item)
        {
            lock (Locker)
                return ContainsInternal(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see
        ///         cref="T:System.Array" />
        ///     , starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (Locker)
                CopyToInternal(array, arrayIndex);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
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

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        int ICollection<T>.Count
        {
            get
            {
                if (UseNotificationMode(true))
                    return NotificationCount;
                return Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IList<T>

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        public int IndexOf(T item)
        {
            lock (Locker)
                return IndexOfInternal(item);
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        public void Insert(int index, T item)
        {
            bool shouldRaiseEvents;
            lock (Locker)
                InsertItemInternal(index, item, false, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            bool shouldRaiseEvents;
            lock (Locker)
                RemoveItemInternal(index, out shouldRaiseEvents);
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
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