#region Copyright
// ****************************************************************************
// <copyright file="OrderedNotifiableCollection.cs">
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Collections
{
    /// <summary>
    ///     Represents the sorted syncronized observable collection, duplicate items (items that compare equal to each other)
    ///     are allows in an OrderedNotifiableCollection.
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    public class OrderedNotifiableCollection<T> : SynchronizedNotifiableCollection<T>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedNotifiableCollection{T}" /> class.
        /// </summary>
        public OrderedNotifiableCollection()
            : base(new OrderedListInternal<T>(), null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedNotifiableCollection{T}" /> class.
        /// </summary>
        public OrderedNotifiableCollection([NotNull] IEnumerable<T> collection, [NotNull] Comparison<T> comparison,
            IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(collection, new DelegateComparer<T>(comparison)), threadManager)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedNotifiableCollection{T}" /> class.
        /// </summary>
        public OrderedNotifiableCollection([NotNull] Comparison<T> comparison, IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(new DelegateComparer<T>(comparison)), threadManager)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedNotifiableCollection{T}" /> class.
        /// </summary>
        public OrderedNotifiableCollection(IComparer<T> comparer = null, IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(comparer), threadManager)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedNotifiableCollection{T}" /> class that contains elements
        ///     copied from the specified collection.
        /// </summary>
        public OrderedNotifiableCollection([NotNull] IEnumerable<T> collection, IComparer<T> comparer = null,
            IThreadManager threadManager = null)
            : base(new OrderedListInternal<T>(collection, comparer), threadManager)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the comparer.
        /// </summary>
        public IComparer<T> Comparer
        {
            get { return Items.Comparer; }
        }

        /// <summary>
        ///     Gets or sets the value that indicates that collection should check index on insert.
        /// </summary>
        public bool ValidateOnInsert { get; set; }

        /// <summary>
        ///     Gets the internal collection.
        /// </summary>
        private new OrderedListInternal<T> Items
        {
            get { return (OrderedListInternal<T>)base.Items; }
        }

        #endregion

        #region Overrides of SyncronizedNotifiableCollection<T>

        /// <summary>
        ///     Initializes default values.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (!(base.Items is OrderedListInternal<T>))
                base.Items = new OrderedListInternal<T>(base.Items);
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