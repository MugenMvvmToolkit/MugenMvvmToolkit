#region Copyright
// ****************************************************************************
// <copyright file="BindingListWrapper.cs">
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Collections
{
    [DebuggerDisplay("Count = {SourceCollection.Count}, NotificationCount = {SourceCollection.NotificationCount}")]
    public class BindingListWrapper<T> : BindingList<T>, IBindingList, INotifyCollectionChanging
    {
        #region Fields

        private readonly object _locker;
        private bool _collectionUpdating;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingList{T}" /> class as a wrapper
        ///     for the specified list.
        /// </summary>
        /// <param name="collection">The list that is wrapped by the new collection.</param>
        public BindingListWrapper([NotNull] SynchronizedNotifiableCollection<T> collection)
            : base(collection)
        {
            _locker = new object();
            CollectionChanged += CollectionOnCollectionChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the source collection.
        /// </summary>
        public SynchronizedNotifiableCollection<T> SourceCollection
        {
            get { return (SynchronizedNotifiableCollection<T>)Items; }
        }

        #endregion

        #region Methods

        private void CollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            try
            {
                lock (_locker)
                {
                    _collectionUpdating = true;
                    ListChangedEventArgs args = null;
                    switch (arg.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            args = new ListChangedEventArgs(ListChangedType.ItemAdded, arg.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            args = new ListChangedEventArgs(ListChangedType.ItemDeleted, arg.OldStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            args = new ListChangedEventArgs(ListChangedType.ItemChanged, arg.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            args = new ListChangedEventArgs(ListChangedType.ItemMoved, arg.NewStartingIndex,
                                arg.OldStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            args = new ListChangedEventArgs(ListChangedType.Reset, -1);
                            break;
                    }
                    OnListChanged(args);
                }
            }
            finally
            {
                _collectionUpdating = false;
            }
        }

        #endregion

        #region Implementation of INotifyCollectionChanging

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { SourceCollection.CollectionChanged += value; }
            remove { SourceCollection.CollectionChanged -= value; }
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { SourceCollection.PropertyChanged += value; }
            remove { SourceCollection.PropertyChanged -= value; }
        }

        /// <summary>
        ///     Occurs before the collection changes.
        /// </summary>
        public event NotifyCollectionChangingEventHandler CollectionChanging
        {
            add { SourceCollection.CollectionChanging += value; }
            remove { SourceCollection.CollectionChanging -= value; }
        }

        #endregion

        #region Overrides of BindingList<T>

        /// <summary>
        ///     Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (_collectionUpdating || e.ListChangedType == ListChangedType.PropertyDescriptorAdded ||
                e.ListChangedType == ListChangedType.PropertyDescriptorChanged ||
                e.ListChangedType == ListChangedType.PropertyDescriptorDeleted ||
                (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor != null))
                base.OnListChanged(e);
        }

        #endregion
    }
}