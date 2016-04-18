#region Copyright

// ****************************************************************************
// <copyright file="FilterableNotifiableCollection.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Collections
{
    [DataContract(IsReference = true, Namespace = ApplicationSettings.DataContractNamespace), Serializable]
    public class FilterableNotifiableCollection<T> : SynchronizedNotifiableCollection<T>
    {
        #region Fields

        [XmlIgnore, NonSerialized]
        private FilterDelegate<T> _filter;

        [XmlIgnore, NonSerialized]
        private OrderedListInternal<int, T> _filterCollection;

        [XmlIgnore, NonSerialized]
        private bool _isSourceNotifiable;

        [XmlIgnore, NonSerialized]
        private OrderedListInternal<int, T> _snapshotFilterCollection;

        #endregion

        #region Constructors

        public FilterableNotifiableCollection()
        {
            IsClearIgnoreFilter = true;
        }

        public FilterableNotifiableCollection(IThreadManager threadManager)
            : base(threadManager)
        {
            IsClearIgnoreFilter = true;
        }

        public FilterableNotifiableCollection([NotNull] IList<T> list, IThreadManager threadManager = null)
            : base(list, threadManager)
        {
            IsClearIgnoreFilter = true;
        }

        public FilterableNotifiableCollection([NotNull] IEnumerable<T> collection, IThreadManager threadManager = null)
            : base(collection, threadManager)
        {
            IsClearIgnoreFilter = true;
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public FilterDelegate<T> Filter
        {
            get { return _filter; }
            set
            {
                if (Equals(_filter, value))
                    return;
                lock (Locker)
                {
                    var oldValue = _filter;
                    var notifiableCollection = Items as INotifiableCollection;
                    if (notifiableCollection != null)
                    {
                        if (value == null)
                        {
                            if (oldValue != null)
                                notifiableCollection.CollectionChanged += OnSourceCollectionChangedSafe;
                        }
                        else if (oldValue == null)
                            notifiableCollection.CollectionChanged -= OnSourceCollectionChangedSafe;
                    }

                    _filter = value;
                    UpdateFilterInternal(value);
                }
            }
        }

        public IList<T> SourceCollection => Items;

        [DataMember]
        public bool IsClearIgnoreFilter { get; set; }

        #endregion

        #region Methods

        public void UpdateFilter()
        {
            lock (Locker)
                UpdateFilterInternal(_filter);
        }

        protected virtual void OnSourceCollectionChangedSafe(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_filter == null)
            {
                if (!EnsureSynchronized())
                    OnCollectionChanged(e, NotificationType.All);
            }
        }

        protected virtual void OnSourceCollectionChangedUnsafe(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (Locker)
            {
                if (_filter == null)
                    return;

                if (IsUiThread())
                {
                    EnsureSynchronized();
                    UpdateItem(_filterCollection, e, NotificationType.All);
                }
                else
                {
                    AddPendingAction(c => ((FilterableNotifiableCollection<T>)c).UpdateItem(((FilterableNotifiableCollection<T>)c)._snapshotFilterCollection, e, NotificationType.Changed), false);
                    UpdateItem(_filterCollection, e, NotificationType.UnsafeChanged);
                }
            }
        }

        protected override int GetCountInternal(IList<T> items)
        {
            if (_filter == null)
                return base.GetCountInternal(items);
            return GetFilterCollection().Count;
        }

        protected override IList<T> OnItemsChanged(IList<T> items)
        {
            _filterCollection = new OrderedListInternal<int, T>();
            var notifiableCollection = items as INotifiableCollection;
            if (notifiableCollection == null)
            {
                var notifyCollectionChanged = items as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    _isSourceNotifiable = true;
                    notifyCollectionChanged.CollectionChanged += OnSourceCollectionChangedUnsafe;
                }
            }
            else
            {
                _isSourceNotifiable = true;
                notifiableCollection.CollectionChangedUnsafe += OnSourceCollectionChangedUnsafe;
                notifiableCollection.CollectionChanged += OnSourceCollectionChangedSafe;
            }
            return base.OnItemsChanged(items);
        }

        protected override void ClearPendingChanges()
        {
            base.ClearPendingChanges();
            _snapshotFilterCollection = null;
        }

        protected override void InitializePendingChanges()
        {
            base.InitializePendingChanges();
            if (_filter != null && _snapshotFilterCollection == null)
                _snapshotFilterCollection = new OrderedListInternal<int, T>(_filterCollection);
        }

        protected override bool ClearItemsInternal(IList<T> items, NotificationType notificationType)
        {
            if (_filter == null)
                return base.ClearItemsInternal(items, GetBaseNotificationType(notificationType));

            if (HasChangingFlag(notificationType))
            {
                var args = GetCollectionChangeArgs();
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }
            var filterCollection = GetFilterCollection();
            if (IsClearIgnoreFilter)
            {
                items.Clear();
                if (_isSourceNotifiable)
                    return true;
                filterCollection.Clear();
                OnCollectionChanged(Empty.ResetEventArgs, notificationType);
            }
            else
            {
                for (int i = 0; i < filterCollection.Count; i++)
                    items.Remove(filterCollection.GetValue(i));
                if (_isSourceNotifiable)
                    return true;
                filterCollection.Clear();
                OnCollectionChanged(Empty.ResetEventArgs, notificationType);
            }
            return true;
        }

        protected override bool RemoveItemInternal(IList<T> items, int index, NotificationType notificationType)
        {
            if (_filter == null)
                return base.RemoveItemInternal(items, index, GetBaseNotificationType(notificationType));

            var filterCollection = GetFilterCollection();
            var originalIndex = filterCollection.GetKey(index);
            T removedItem = items[originalIndex];
            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Remove, removedItem, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }
            items.RemoveAt(originalIndex);
            if (_isSourceNotifiable)
                return true;
            UpdateFilterItems(filterCollection, originalIndex, -1);
            filterCollection.RemoveAt(index);
            OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index), notificationType);
            return true;
        }

        protected override bool SetItemInternal(IList<T> items, int index, T item, NotificationType notificationType)
        {
            if (_filter == null)
                return base.SetItemInternal(items, index, item, GetBaseNotificationType(notificationType));

            var filterCollection = GetFilterCollection();
            var originalIndex = filterCollection.GetKey(index);
            T oldItem = items[originalIndex];
            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Replace, oldItem, item, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }

            items[originalIndex] = item;
            if (_isSourceNotifiable)
                return true;

            if (_filter(item))
            {
                filterCollection.Values[index] = item;
                OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index), notificationType);
            }
            else
            {
                filterCollection.RemoveAt(index);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index), notificationType);
            }
            return true;
        }

        protected override int InsertItemInternal(IList<T> items, int index, T item, bool isAdd, NotificationType notificationType)
        {
            if (_filter == null)
                return base.InsertItemInternal(items, index, item, isAdd, GetBaseNotificationType(notificationType));

            var filterCollection = GetFilterCollection();
            int originalIndex = index;
            if (isAdd)
                originalIndex = items.Count;
            else if (filterCollection.Count != 0)
            {
                if (index != 0)
                    originalIndex = filterCollection.GetKey(index - 1) + 1;
                else
                    originalIndex = filterCollection.GetKey(index);
            }
            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Add, item, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return -1;
            }

            items.Insert(originalIndex, item);
            if (_isSourceNotifiable)
                return originalIndex;

            UpdateFilterItems(filterCollection, originalIndex, 1);
            if (!_filter(item))
                return -1;
            originalIndex = filterCollection.Add(originalIndex, item);
            OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index), notificationType);
            return originalIndex;
        }

        protected override int IndexOfInternal(IList<T> items, T item)
        {
            if (_filter == null)
                return base.IndexOfInternal(items, item);
            return GetFilterCollection().IndexOfValue(item);
        }

        protected override T GetItemInternal(IList<T> items, int index)
        {
            if (_filter == null)
                return base.GetItemInternal(items, index);
            return GetFilterCollection().GetValue(index);
        }

        protected override bool ContainsInternal(IList<T> items, T item)
        {
            return IsSatisfy(item) && base.ContainsInternal(items, item);
        }

        private OrderedListInternal<int, T> GetFilterCollection()
        {
            if (IsUiThread() && _snapshotFilterCollection != null)
                return _snapshotFilterCollection;
            return _filterCollection;
        }

        private void UpdateFilterInternal(FilterDelegate<T> value)
        {
            if (IsUiThread())
            {
                //Clearing changes collection will be recreated
                ClearPendingChanges();
                UpdateFilterInternal(_filterCollection, value, NotificationType.All);
            }
            else
            {
                AddPendingAction(c => ((FilterableNotifiableCollection<T>)c).RaiseResetInternal(), true);
                UpdateFilterInternal(_filterCollection, value, NotificationType.UnsafeChanged);
            }
        }

        private static void UpdateFilterItems(OrderedListInternal<int, T> filterCollection, int index, int value)
        {
            if (filterCollection.Count == 0) return;
            int start = filterCollection.IndexOfKey(index);
            if (start == -1)
            {
                if (filterCollection.Keys[filterCollection.Count - 1] < index)
                    return;
                for (int i = 0; i < filterCollection.Count; i++)
                {
                    int key = filterCollection.Keys[i];
                    if (key < index)
                        continue;
                    filterCollection.Keys[i] = key + value;
                }
                return;
            }
            for (int i = start; i < filterCollection.Count; i++)
                filterCollection.Keys[i] += value;
        }

        private bool IsSatisfy(T item)
        {
            FilterDelegate<T> filterDelegate = _filter;
            return filterDelegate == null || filterDelegate(item);
        }

        private void UpdateFilterInternal(OrderedListInternal<int, T> filterCollection, FilterDelegate<T> filter, NotificationType notificationType)
        {
            var currentFilter = filter;
            filterCollection.Clear();
            var items = GetItems();
            if (currentFilter != null)
            {
                for (var index = 0; index < items.Count; index++)
                {
                    var item = items[index];
                    if (currentFilter(item))
                        filterCollection.Add(index, item);
                }
            }
            OnCollectionChanged(Empty.ResetEventArgs, notificationType);
        }

        private NotificationType GetBaseNotificationType(NotificationType notificationType)
        {
            if (_isSourceNotifiable)
                return notificationType & ~NotificationType.Changed;
            return notificationType;
        }

        private void UpdateItem(OrderedListInternal<int, T> filterCollection, NotifyCollectionChangedEventArgs e, NotificationType notificationType)
        {
            int filterIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var item = (T)e.NewItems[0];
                    UpdateFilterItems(filterCollection, e.NewStartingIndex, 1);

                    if (!_filter(item))
                        return;
                    filterIndex = filterCollection.Add(e.NewStartingIndex, item);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, filterIndex), notificationType);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    filterIndex = filterCollection.IndexOfKey(e.OldStartingIndex);
                    UpdateFilterItems(filterCollection, e.OldStartingIndex, -1);
                    if (filterIndex == -1)
                        return;
                    filterCollection.RemoveAt(filterIndex);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems[0], filterIndex), notificationType);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    filterIndex = filterCollection.IndexOfKey(e.NewStartingIndex);
                    if (filterIndex == -1) return;

                    var newItem = (T)e.NewItems[0];
                    if (_filter(newItem))
                    {
                        T oldItem = filterCollection.GetValue(filterIndex);
                        filterCollection.Values[filterIndex] = newItem;
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems[0], oldItem, filterIndex), notificationType);
                    }
                    else
                    {
                        T oldValue = filterCollection.GetValue(filterIndex);
                        filterCollection.RemoveAt(filterIndex);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValue, filterIndex), notificationType);
                    }
                    break;
                default:
                    UpdateFilterInternal(filterCollection, _filter, notificationType);
                    break;
            }
        }

        #endregion
    }
}