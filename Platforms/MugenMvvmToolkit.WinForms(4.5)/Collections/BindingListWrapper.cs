#region Copyright

// ****************************************************************************
// <copyright file="BindingListWrapper.cs">
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
using System.Linq;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinForms.Collections
{
    public class BindingListWrapper<T> : BindingList<T>, IBindingList, INotifiableCollection, INotifiableCollection<T>
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

        public INotifiableCollection<T> SourceCollection => (INotifiableCollection<T>)Items;

        public bool IsNotificationsSuspended => SourceCollection.IsNotificationsSuspended;

        int ICollection.Count
        {
            get
            {
                var collection = SourceCollection as ICollection;
                if (collection == null)
                    return Count;
                return collection.Count;
            }
        }

        object IList.this[int index]
        {
            get
            {
                var list = SourceCollection as IList ?? this;
                return list[index];
            }
            set
            {
                var list = SourceCollection as IList ?? this;
                list[index] = value;
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

        void INotifiableCollection.AddRange(IEnumerable collection, bool suspendNotifications)
        {
            AddRange(collection.Cast<T>(), suspendNotifications);
        }

        void INotifiableCollection.RemoveRange(IEnumerable collection, bool suspendNotifications)
        {
            RemoveRange(collection.Cast<T>(), suspendNotifications);
        }

        public event NotifyCollectionChangedEventHandler CollectionChangedUnsafe
        {
            add { SourceCollection.CollectionChangedUnsafe += value; }
            remove { SourceCollection.CollectionChangedUnsafe -= value; }
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

        public void AddRange(IEnumerable<T> collection, bool suspendNotifications = true)
        {
            foreach (var item in collection)
                Add(item);
        }

        public void RemoveRange(IEnumerable<T> collection, bool suspendNotifications = true)
        {
            foreach (var item in collection)
                Remove(item);
        }

        public void Update(IEnumerable<T> items)
        {
            SourceCollection.Update(items);
        }

        public T[] ToArray()
        {
            return SourceCollection.ToArray();
        }

        #endregion
    }
}