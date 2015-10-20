#region Copyright

// ****************************************************************************
// <copyright file="BindingListWrapper.cs">
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinForms.Collections
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class BindingListWrapper<T> : BindingList<T>, IBindingList, INotifiableCollection<T>
    {
        #region Fields

        private bool _collectionUpdating;

        #endregion

        #region Constructors

        public BindingListWrapper(INotifiableCollection<T> collection = null)
            : base(collection ?? new SynchronizedNotifiableCollection<T>())
        {
            CollectionChanged += CollectionOnCollectionChanged;
        }

        #endregion

        #region Properties

        public INotifiableCollection<T> SourceCollection
        {
            get { return (INotifiableCollection<T>)Items; }
        }

        public bool IsNotificationsSuspended
        {
            get { return SourceCollection.IsNotificationsSuspended; }
        }

        private string DebuggerDisplay
        {
            get
            {
                int c = 0;
                var collection = SourceCollection as SynchronizedNotifiableCollection<T>;
                if (collection != null)
                    c = collection.NotificationCount;
                return string.Format("Count = {0}, NotificationCount = {1}", SourceCollection.Count, c);
            }
        }

        #endregion

        #region Methods

        private void CollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            try
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
            finally
            {
                _collectionUpdating = false;
            }
        }

        #region Overrides of BindingList<T>

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (_collectionUpdating || e.ListChangedType == ListChangedType.PropertyDescriptorAdded ||
                e.ListChangedType == ListChangedType.PropertyDescriptorChanged ||
                e.ListChangedType == ListChangedType.PropertyDescriptorDeleted ||
                (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor != null))
                base.OnListChanged(e);
        }

        #endregion

        #endregion

        #region Implementation of interfaces

        public IDisposable SuspendNotifications()
        {
            return SourceCollection.SuspendNotifications();
        }

        public void RaiseReset()
        {
            SourceCollection.RaiseReset();
        }

        public void AddRange(IEnumerable collection)
        {
            ((INotifiableCollection)SourceCollection).AddRange(collection);
        }

        public void RemoveRange(IEnumerable collection)
        {
            ((INotifiableCollection)SourceCollection).RemoveRange(collection);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            SourceCollection.AddRange(collection);
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            SourceCollection.RemoveRange(collection);
        }

        public void Update(IEnumerable<T> items)
        {
            SourceCollection.Update(items);
        }

        public bool Replace(T oldValue, T newValue)
        {
            return SourceCollection.Replace(oldValue, newValue);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { SourceCollection.CollectionChanged += value; }
            remove { SourceCollection.CollectionChanged -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { SourceCollection.PropertyChanged += value; }
            remove { SourceCollection.PropertyChanged -= value; }
        }

        public event NotifyCollectionChangingEventHandler CollectionChanging
        {
            add { SourceCollection.CollectionChanging += value; }
            remove { SourceCollection.CollectionChanging -= value; }
        }

        #endregion
    }
}