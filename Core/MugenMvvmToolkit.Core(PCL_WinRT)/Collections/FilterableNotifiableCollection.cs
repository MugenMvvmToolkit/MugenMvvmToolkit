#region Copyright

// ****************************************************************************
// <copyright file="FilterableNotifiableCollection.cs">
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Collections
{
    /// <summary>
    ///     Represents the collection which supports custom filter and <see cref="INotifyCollectionChanged" />,
    ///     <see
    ///         cref="INotifyPropertyChanged" />
    ///     events.
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    [DataContract(IsReference = true, Namespace = ApplicationSettings.DataContractNamespace), Serializable]
    public class FilterableNotifiableCollection<T> : SynchronizedNotifiableCollection<T>
    {
        #region Fields

        [XmlIgnore, NonSerialized]
        private FilterDelegate<T> _filter;
        [XmlIgnore, NonSerialized]
        private OrderedListInternal<int, T> _filterCollection;

        [XmlIgnore, NonSerialized]
        private bool _isSuspendedInternal;

        [XmlIgnore, NonSerialized]
        private INotifyCollectionChanged _notifyCollectionChanged;
        [XmlIgnore, NonSerialized]
        private INotifyCollectionChanging _notifyCollectionChanging;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterableNotifiableCollection{T}" /> class.
        /// </summary>
        public FilterableNotifiableCollection()
            : base(new ObservableCollection<T>(), null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterableNotifiableCollection{T}" /> class.
        /// </summary>
        public FilterableNotifiableCollection(IThreadManager threadManager)
            : base(new ObservableCollection<T>(), threadManager)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterableNotifiableCollection{T}" /> class.
        /// </summary>
        public FilterableNotifiableCollection([NotNull] IList<T> list, IThreadManager threadManager = null)
            : base(list.IsReadOnly ? new ObservableCollection<T>(list) : list, threadManager)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterableNotifiableCollection{T}" /> class.
        /// </summary>
        public FilterableNotifiableCollection([NotNull] IEnumerable<T> collection, IThreadManager threadManager = null)
            : base(new ObservableCollection<T>(collection), threadManager)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        [XmlIgnore]
        public FilterDelegate<T> Filter
        {
            get { return _filter; }
            set
            {
                if (Equals(_filter, value)) return;
                bool shouldRaiseEvents;
                lock (Locker)
                {
                    _filter = value;
                    UpdateFilterInternal(out shouldRaiseEvents);
                }
                if (shouldRaiseEvents)
                    RaiseEvents();
            }
        }

        /// <summary>
        ///     Gets the source <see cref="IList{T}" />.
        /// </summary>
        public IList<T> SourceCollection
        {
            get { return Items; }
        }

        /// <summary>
        ///     Indicates that always need reset internal collection on any changes in original collection.
        /// </summary>
        public bool AlwaysReset { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates a filter.
        /// </summary>
        public void UpdateFilter()
        {
            bool shouldRaiseEvents;
            lock (Locker)
            {
                UpdateFilterInternal(out shouldRaiseEvents);
            }
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        /// <summary>
        ///     Updates a filter.
        /// </summary>
        protected virtual void UpdateFilterInternal(out bool shouldRaiseEvents)
        {
            FilterDelegate<T> currentFilter = _filter;
            if (currentFilter != null)
            {
                _filterCollection.Clear();
                for (int index = 0; index < Items.Count; index++)
                {
                    T item = Items[index];
                    if (currentFilter(item))
                        _filterCollection.Add(index, item);
                }
            }
            EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            shouldRaiseEvents = true;
        }

        /// <summary>
        ///     Occurs when source collection changed.
        /// </summary>
        protected virtual void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool shouldRaiseEvents;
            lock (Locker)
            {
                if (_filter == null)
                {
                    EventsTracker.AddEvent(e);
                    shouldRaiseEvents = true;
                }
                else if (AlwaysReset || (e.NewItems != null && e.NewItems.Count > 1) ||
                         (e.OldItems != null && e.OldItems.Count > 1))
                {
                    UpdateFilterInternal(out shouldRaiseEvents);
                }
                else
                {
                    UpdateItem(e, out shouldRaiseEvents);
                }
            }
            if (shouldRaiseEvents)
                RaiseEvents();
        }

        private void UpdateItem(NotifyCollectionChangedEventArgs e, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            int filterIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var item = (T)e.NewItems[0];
                    UpdateFilterItems(e.NewStartingIndex, 1);

                    if (!_filter(item)) return;
                    filterIndex = _filterCollection.Add(e.NewStartingIndex, item);
                    EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item,
                        filterIndex));
                    shouldRaiseEvents = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    filterIndex = _filterCollection.IndexOfKey(e.OldStartingIndex);
                    UpdateFilterItems(e.OldStartingIndex, -1);
                    if (filterIndex == -1) return;

                    _filterCollection.RemoveAt(filterIndex);
                    EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                        e.OldItems[0], filterIndex));
                    shouldRaiseEvents = true;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    filterIndex = _filterCollection.IndexOfKey(e.NewStartingIndex);
                    if (filterIndex == -1) return;

                    var newItem = (T)e.NewItems[0];
                    if (_filter(newItem))
                    {
                        T oldItem = _filterCollection.GetValue(filterIndex);
                        _filterCollection.Values[filterIndex] = newItem;
                        EventsTracker.AddEvent(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                e.NewItems[0], oldItem, filterIndex));
                    }
                    else
                    {
                        T oldValue = _filterCollection.GetValue(filterIndex);
                        _filterCollection.RemoveAt(filterIndex);
                        EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove, oldValue,
                            filterIndex));
                    }
                    shouldRaiseEvents = true;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    UpdateFilterInternal(out shouldRaiseEvents);
                    break;
                default:
                    //For Move support.
                    if (e.Action.ToString("G") != "Move")
                        break;
                    var movedItem = (T)e.NewItems[0];
                    filterIndex = _filterCollection.IndexOfKey(e.OldStartingIndex);
                    UpdateFilterItems(e.OldStartingIndex, -1);
                    UpdateFilterItems(e.NewStartingIndex, 1);
                    if (filterIndex == -1 || !_filter(movedItem)) return;

                    _filterCollection.RemoveAt(filterIndex);
                    var newIndex = _filterCollection.Add(e.NewStartingIndex, movedItem);
                    EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, e.OldItems[0],
                        filterIndex));
                    EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                        movedItem,
                        newIndex));
                    shouldRaiseEvents = true;
                    break;
            }
        }

        private bool IsSatisfy(T item)
        {
            FilterDelegate<T> filterDelegate = _filter;
            return filterDelegate == null || filterDelegate(item);
        }

        private void UpdateFilterItems(int index, int value)
        {
            if (_filterCollection.Count == 0) return;
            int start = _filterCollection.IndexOfKey(index);
            if (start == -1)
            {
                if (_filterCollection.Keys[_filterCollection.Count - 1] < index) return;
                for (int i = 0; i < _filterCollection.Count; i++)
                {
                    int key = _filterCollection.Keys[i];
                    if (key < index) continue;
                    _filterCollection.Keys[i] = key + value;
                }
                return;
            }
            for (int i = start; i < _filterCollection.Count; i++)
                _filterCollection.Keys[i] += value;
        }

        private ActionToken SuspendInternal()
        {
            //changing value that can cause deadlock.
            var notifiableCollection = Items as SynchronizedNotifiableCollection<T>;
            if (notifiableCollection != null && notifiableCollection.ExecutionMode == ExecutionMode.SynchronousOnUiThread)
                notifiableCollection.ExecutionMode = ExecutionMode.None;

            _isSuspendedInternal = true;
            return new ActionToken(o => ((FilterableNotifiableCollection<T>)o)._isSuspendedInternal = false, this);
        }

        private IEnumerable<T> GetValues()
        {
            return _filterCollection.Values.Take(_filterCollection.Count);
        }

        #endregion

        #region Overrides of SynchronizedNotifiableCollection<T>

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        protected override int CountInternal
        {
            get
            {
                if (_filter == null)
                    return Items.Count;
                return _filterCollection.Count;
            }
        }

        /// <summary>
        ///     Initializes default values.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            var collection = Items as SynchronizedNotifiableCollection<T>;
            if (collection != null)
                collection.ExecutionMode = Models.ExecutionMode.None;
            _filterCollection = new OrderedListInternal<int, T>();
            _notifyCollectionChanged = Items as INotifyCollectionChanged;
            _notifyCollectionChanging = Items as INotifyCollectionChanging;
            if (_notifyCollectionChanged != null)
                _notifyCollectionChanged.CollectionChanged += OnSourceCollectionChanged;
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItemsInternal(out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = true;
            if (_notifyCollectionChanging != null)
            {
                using (SuspendInternal())
                    Items.Clear();
                return;
            }
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs();
            OnCollectionChanging(args);
            if (args.Cancel)
            {
                shouldRaiseEvents = false;
                return;
            }

            if (_notifyCollectionChanged != null)
            {
                using (SuspendInternal())
                    Items.Clear();
                return;
            }
            Items.Clear();
            UpdateFilterInternal(out shouldRaiseEvents);
        }

        /// <summary>
        ///     Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the element to remove.
        /// </param>
        /// <param name="shouldRaiseEvents"></param>
        protected override void RemoveItemInternal(int index, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = true;
            int originalIndex = index;
            if (_filter != null)
                originalIndex = _filterCollection.GetKey(index);
            if (_notifyCollectionChanging != null)
            {
                using (SuspendInternal())
                    Items.RemoveAt(originalIndex);
                return;
            }

            T removedItem = Items[originalIndex];
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Remove,
                removedItem, index);
            OnCollectionChanging(args);
            if (args.Cancel)
            {
                shouldRaiseEvents = false;
                return;
            }

            if (_notifyCollectionChanged != null)
            {
                using (SuspendInternal())
                    Items.RemoveAt(originalIndex);
                return;
            }

            Items.RemoveAt(originalIndex);
            UpdateFilterItems(originalIndex, -1);
            _filterCollection.RemoveAt(index);
            EventsTracker.AddEvent(args.ChangedEventArgs);
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
        protected override void SetItemInternal(int index, T item, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = true;
            int originalIndex = index;
            if (_filter != null)
                originalIndex = _filterCollection.GetKey(index);
            if (_notifyCollectionChanging != null)
            {
                using (SuspendInternal())
                    Items[originalIndex] = item;
                return;
            }
            T oldItem = Items[originalIndex];
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Replace,
                oldItem, item,
                index);
            OnCollectionChanging(args);
            if (args.Cancel)
            {
                shouldRaiseEvents = false;
                return;
            }

            if (_notifyCollectionChanged != null)
            {
                using (SuspendInternal())
                    Items[originalIndex] = item;
                return;
            }

            Items[originalIndex] = item;
            if (_filter == null || _filter(item))
            {
                _filterCollection.Values[index] = item;
                EventsTracker.AddEvent(args.ChangedEventArgs);
            }
            else
            {
                _filterCollection.RemoveAt(index);
                EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,
                    index));
            }
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
        protected override int InsertItemInternal(int index, T item, bool isAdd, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = true;
            int originalIndex = index;
            if (isAdd)
                originalIndex = Items.Count;
            else if (_filter != null && _filterCollection.Count != 0)
            {
                if (index != 0)
                    originalIndex = _filterCollection.GetKey(index - 1) + 1;
                else
                    originalIndex = _filterCollection.GetKey(index);
            }
            if (_notifyCollectionChanging != null)
            {
                using (SuspendInternal())
                    Items.Insert(originalIndex, item);
                return IndexOfInternal(item);
            }
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Add, item,
                index);
            OnCollectionChanging(args);
            if (args.Cancel)
            {
                shouldRaiseEvents = false;
                return -1;
            }

            if (_notifyCollectionChanged != null)
            {
                using (SuspendInternal())
                    Items.Insert(originalIndex, item);
                return IndexOfInternal(item);
            }

            Items.Insert(originalIndex, item);
            originalIndex = Items.IndexOf(item);
            if (originalIndex == -1)
            {
                shouldRaiseEvents = false;
                return -1;
            }

            UpdateFilterItems(originalIndex, 1);
            if (_filter != null && !_filter(item))
            {
                shouldRaiseEvents = false;
                return -1;
            }
            originalIndex = _filterCollection.Add(originalIndex, item);
            EventsTracker.AddEvent(args.ChangedEventArgs);
            return originalIndex;
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
        protected override int IndexOfInternal(T item)
        {
            if (_filter == null)
                return Items.IndexOf(item);
            return _filterCollection.IndexOfValue(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see
        ///         cref="T:System.Array" />
        ///     , starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        protected override void CopyToInternal(Array array, int index)
        {
            for (int i = index; i < CountInternal; i++)
                array.SetValue(GetItemInternal(i), i);
        }

        /// <summary>
        ///     Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <returns>An instance of T.</returns>
        protected override T GetItemInternal(int index)
        {
            if (index < 0 || index > CountInternal)
                throw ExceptionManager.IntOutOfRangeCollection("index");

            if (_filter == null)
                return Items[index];
            return _filterCollection.GetValue(index);
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
        protected override bool ContainsInternal(T item)
        {
            return IsSatisfy(item) && Items.Contains(item);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        protected override IEnumerator<T> GetEnumeratorInternal()
        {
            if (_filter == null)
                return Items.GetEnumerator();
            return GetValues().GetEnumerator();
        }

        /// <summary>
        ///     Raises events from queue of events.
        /// </summary>
        protected override void RaiseEvents()
        {
            if (_isSuspendedInternal) return;
            base.RaiseEvents();
        }

        /// <summary>
        ///     Occurs before the collection changes.
        /// </summary>
        public override event NotifyCollectionChangingEventHandler CollectionChanging
        {
            add
            {
                if (_notifyCollectionChanging == null)
                    base.CollectionChanging += value;
                else
                    _notifyCollectionChanging.CollectionChanging += value;
            }
            remove
            {
                if (_notifyCollectionChanging == null)
                    base.CollectionChanging -= value;
                else
                    _notifyCollectionChanging.CollectionChanging -= value;
            }
        }

        #endregion
    }
}