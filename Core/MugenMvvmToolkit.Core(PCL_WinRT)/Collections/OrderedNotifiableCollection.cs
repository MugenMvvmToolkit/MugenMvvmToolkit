#region Copyright

// ****************************************************************************
// <copyright file="OrderedNotifiableCollection.cs">
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

        public IComparer<T> Comparer => Items.Comparer;

        public bool ValidateOnInsert { get; set; }

        private new OrderedListInternal<T> Items => (OrderedListInternal<T>)base.Items;

        #endregion

        #region Overrides of SynchronizedNotifiableCollection<T>

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (!(base.Items is OrderedListInternal<T>))
                base.Items = new OrderedListInternal<T>(base.Items);
        }

        protected override void SetItemInternal(int index, T item, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            T oldItem = Items[index];
            NotifyCollectionChangingEventArgs args = GetCollectionChangeArgs(NotifyCollectionChangedAction.Replace,
                oldItem, item, index);
            OnCollectionChanging(args);
            if (args.Cancel) return;

            Items.RemoveAt(index);
            int newIndex = Items.Add(item);
            EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem,
                index));
            EventsTracker.AddEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item,
                newIndex));
            shouldRaiseEvents = true;
        }

        protected override int InsertItemInternal(int index, T item, bool isAdd, out bool shouldRaiseEvents)
        {
            shouldRaiseEvents = false;
            if (ValidateOnInsert)
            {
                if (isAdd)
                    index = Items.GetInsertIndex(item);
            }
            else
                index = Items.GetInsertIndex(item);

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

        #endregion
    }
}
