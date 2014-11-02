using System.Collections;
using System.Collections.Specialized;
using JetBrains.Annotations;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ItemsSourceCollectionViewSource : CollectionViewSourceBase
    {
        #region Fields

        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private IEnumerable _itemsSource;

        #endregion

        #region Constructors

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
            set { SetItemsSource(value); }
        }

        #endregion

        #region Overrides of CollectionViewSourceBase

        public override int GetItemsCount(UICollectionView collectionView, int section)
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

        protected override void SetSelectedCellByItem(object selectedItem)
        {
            if (selectedItem == null)
                ClearSelection();
            else
            {
                int i = ItemsSource.IndexOf(selectedItem);
                if (i < 0)
                    ClearSelection();
                else
                    CollectionView.SelectItem(NSIndexPath.FromRowSection(i, 0), UseAnimations, ScrollPosition);
            }
        }

        #endregion

        #region Methods

        protected virtual void SetItemsSource(IEnumerable value)
        {
            if (ReferenceEquals(value, _itemsSource))
                return;
            if (_weakHandler == null)
                _itemsSource = value;
            else
            {
                IEnumerable oldValue = _itemsSource;
                var notifyCollectionChanged = oldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged -= _weakHandler;
                _itemsSource = value;
                notifyCollectionChanged = value as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged += _weakHandler;
            }
            ReloadData();
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (!TryUpdateItems(args))
                ReloadData();
        }

        protected bool TryUpdateItems(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    NSIndexPath[] newIndexPaths = PlatformExtensions.CreateNSIndexPathArray(args.NewStartingIndex,
                        args.NewItems.Count);
                    CollectionView.InsertItems(newIndexPaths);
                    return true;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in args.OldItems)
                        ItemDeselected(oldItem);
                    NSIndexPath[] oldIndexPaths = PlatformExtensions.CreateNSIndexPathArray(args.OldStartingIndex,
                            args.OldItems.Count);
                    CollectionView.DeleteItems(oldIndexPaths);
                    return true;
                case NotifyCollectionChangedAction.Move:
                    if (args.NewItems.Count != 1 && args.OldItems.Count != 1)
                        return false;

                    NSIndexPath oldIndexPath = NSIndexPath.FromRowSection(args.OldStartingIndex, 0);
                    NSIndexPath newIndexPath = NSIndexPath.FromRowSection(args.NewStartingIndex, 0);
                    CollectionView.MoveItem(oldIndexPath, newIndexPath);
                    return true;
                case NotifyCollectionChangedAction.Replace:
                    if (args.NewItems.Count != args.OldItems.Count)
                        return false;
                    NSIndexPath indexPath = NSIndexPath.FromRowSection(args.NewStartingIndex, 0);
                    CollectionView.ReloadItems(new[] { indexPath });
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}