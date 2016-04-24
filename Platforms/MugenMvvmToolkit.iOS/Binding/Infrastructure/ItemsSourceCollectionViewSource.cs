#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceCollectionViewSource.cs">
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
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public class ItemsSourceCollectionViewSource : CollectionViewSourceBase
    {
        #region Fields

        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private IEnumerable _itemsSource;

        #endregion

        #region Constructors

        protected ItemsSourceCollectionViewSource(IntPtr handle)
            : base(handle)
        {
        }

        public ItemsSourceCollectionViewSource([NotNull] UICollectionView collectionView,
            string itemTemplate = AttachedMemberConstants.ItemTemplate)
            : base(collectionView, itemTemplate)
        {
            _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this,
                (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
        }

        #endregion

        #region Properties

        public virtual IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set { SetItemsSource(value, true); }
        }

        #endregion

        #region Overrides of CollectionViewSourceBase

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            if (ItemsSource == null)
                return 0;
            return ItemsSource.Count();
        }

        protected override object GetItemAt(NSIndexPath indexPath)
        {
            if (indexPath == null || ItemsSource == null)
                return null;
            return ItemsSource.ElementAtIndex(indexPath.Row);
        }

        protected override void SetSelectedCellByItem(UICollectionView collectionView, object selectedItem)
        {
            if (selectedItem == null)
                ClearSelection(collectionView);
            else
            {
                int i = ItemsSource.IndexOf(selectedItem);
                if (i < 0)
                    ClearSelection(collectionView);
                else
                    collectionView.SelectItem(NSIndexPath.FromRowSection(i, 0), UseAnimations, ScrollPosition);
            }
        }

        protected override void OnDisposeController(object sender, EventArgs eventArgs)
        {
            SetItemsSource(null, false);
            base.OnDisposeController(sender, eventArgs);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SetItemsSource(null, false);
        }

        #endregion

        #region Methods

        protected virtual void SetItemsSource(IEnumerable value, bool reload)
        {
            if (ReferenceEquals(value, _itemsSource) || !this.IsAlive())
                return;
            var notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged -= _weakHandler;
            _itemsSource = value;
            if (reload)
                ReloadData();
            notifyCollectionChanged = value as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
                notifyCollectionChanged.CollectionChanged += _weakHandler;
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!this.IsAlive())
                return;
            if (!TryUpdateItems(args))
                ReloadData();
            if (args.Action == NotifyCollectionChangedAction.Remove ||
                args.Action == NotifyCollectionChangedAction.Replace ||
                args.Action == NotifyCollectionChangedAction.Reset)
                ClearSelectedItemIfNeed();
        }

        protected bool TryUpdateItems(NotifyCollectionChangedEventArgs args)
        {
            var collectionView = CollectionView;
            if (collectionView == null)
                return false;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    NSIndexPath[] newIndexPaths = PlatformExtensions.CreateNSIndexPathArray(args.NewStartingIndex,
                        args.NewItems.Count);
                    collectionView.InsertItems(newIndexPaths);
                    newIndexPaths.DisposeEx();
                    return true;
                case NotifyCollectionChangedAction.Remove:
                    NSIndexPath[] oldIndexPaths = PlatformExtensions.CreateNSIndexPathArray(args.OldStartingIndex, args.OldItems.Count);
                    collectionView.DeleteItems(oldIndexPaths);
                    oldIndexPaths.DisposeEx();
                    return true;
                case NotifyCollectionChangedAction.Move:
                    if (args.NewItems.Count != 1 && args.OldItems.Count != 1)
                        return false;

                    NSIndexPath oldIndexPath = NSIndexPath.FromRowSection(args.OldStartingIndex, 0);
                    NSIndexPath newIndexPath = NSIndexPath.FromRowSection(args.NewStartingIndex, 0);
                    collectionView.MoveItem(oldIndexPath, newIndexPath);
                    oldIndexPath.Dispose();
                    newIndexPath.Dispose();
                    return true;
                case NotifyCollectionChangedAction.Replace:
                    if (args.NewItems.Count != args.OldItems.Count)
                        return false;
                    NSIndexPath indexPath = NSIndexPath.FromRowSection(args.NewStartingIndex, 0);
                    collectionView.ReloadItems(new[] { indexPath });
                    indexPath.Dispose();
                    return true;
                default:
                    return false;
            }
        }

        private void ClearSelectedItemIfNeed()
        {
            if (SelectedItem != null && ItemsSource != null && ItemsSource.IndexOf(SelectedItem) < 0)
                SelectedItem = null;
        }

        #endregion
    }
}
