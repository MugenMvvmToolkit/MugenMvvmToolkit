#region Copyright

// ****************************************************************************
// <copyright file="OrderedNotifiableCollection.cs">
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Collections
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    public class OrderedNotifiableCollection<T> : SynchronizedNotifiableCollection<T>
    {
        #region Constructors

        public OrderedNotifiableCollection()
            : base(new OrderedListInternal<T>(), null)
        {
        }

        public OrderedNotifiableCollection([NotNull] IEnumerable<T> collection, [NotNull] Comparison<T> comparison,
            IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(collection, new DelegateComparer<T>(comparison)), threadManager)
        {
        }

        public OrderedNotifiableCollection([NotNull] Comparison<T> comparison, IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(new DelegateComparer<T>(comparison)), threadManager)
        {
        }

        public OrderedNotifiableCollection(IComparer<T> comparer = null, IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(comparer), threadManager)
        {
        }

        public OrderedNotifiableCollection([NotNull] IEnumerable<T> collection, IComparer<T> comparer = null,
            IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(collection, comparer), threadManager)
        {
        }

        #endregion

        #region Properties

        public IComparer<T> Comparer => ((OrderedListInternal<T>)Items).Comparer;

        public bool ValidateOnInsert { get; set; }

        #endregion

        #region Overrides of SynchronizedNotifiableCollection<T>

        protected override IList<T> OnItemsChanged(IList<T> items)
        {
            if (items is OrderedListInternal<T>)
                return items;
            return new OrderedListInternal<T>(items);
        }

        protected override IList<T> CreateSnapshotCollection(IList<T> items)
        {
            return new OrderedListInternal<T>(items, Comparer);
        }

        protected override bool SetItemInternal(IList<T> items, int index, T item, NotificationType notificationType)
        {
            var orderedList = (OrderedListInternal<T>)items;
            T oldItem = orderedList[index];

            if (HasChangingFlag(notificationType))
            {
                NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Replace, oldItem, item, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return false;
            }
            orderedList.RemoveAt(index);
            int newIndex = orderedList.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index), notificationType);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, newIndex), notificationType);
            return true;
        }

        protected override int InsertItemInternal(IList<T> items, int index, T item, bool isAdd, NotificationType notificationType)
        {
            var orderedList = (OrderedListInternal<T>)items;
            if (isAdd || !ValidateOnInsert)
                index = orderedList.GetInsertIndex(item);

            NotifyCollectionChangingEventArgs args = null;
            if (HasChangingFlag(notificationType))
            {
                args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Add, item, index);
                OnCollectionChanging(args);
                if (args.Cancel)
                    return -1;
            }
            orderedList.Insert(index, item);
            OnCollectionChanged(args?.ChangedEventArgs ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index), notificationType);
            return index;
        }

        #endregion
    }
}
