using System;
using Foundation;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Ios.Interfaces;
using UIKit;

namespace MugenMvvm.Ios.Collections
{
    public class MugenCollectionViewSource : UICollectionViewSource
    {
        #region Fields

        private const int InitializedStateMask = 1;

        #endregion

        #region Constructors

        public MugenCollectionViewSource(UICollectionView collectionView, ICellTemplateSelector itemTemplateSelector)
            : this(new ItemsSourceBindableCollectionAdapter(new CollectionViewAdapter(collectionView), itemTemplateSelector as IItemsSourceEqualityComparer), itemTemplateSelector)
        {
        }

        public MugenCollectionViewSource(ItemsSourceBindableCollectionAdapter collectionAdapter, ICellTemplateSelector itemTemplateSelector)
        {
            Should.NotBeNull(collectionAdapter, nameof(collectionAdapter));
            Should.NotBeNull(itemTemplateSelector, nameof(itemTemplateSelector));
            CollectionAdapter = collectionAdapter;
            ItemTemplateSelector = itemTemplateSelector;
        }

        #endregion

        #region Properties

        public ItemsSourceBindableCollectionAdapter CollectionAdapter { get; }

        public ICellTemplateSelector ItemTemplateSelector { get; }

        #endregion

        #region Methods

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = (UICollectionViewCell) collectionView.DequeueReusableCell(ItemTemplateSelector.GetIdentifier(collectionView, item), indexPath);
            cell.BindableMembers().SetDataContext(item);
            if ((cell.Tag & InitializedStateMask) == InitializedStateMask)
            {
                cell.Tag |= InitializedStateMask;
                cell.BindableMembers().SetParent(collectionView);
                ItemTemplateSelector.OnCellCreated(collectionView, cell);
            }

            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section) => CollectionAdapter.Count;

        protected virtual object? GetItemAt(NSIndexPath indexPath) => CollectionAdapter[indexPath.Row];

        #endregion
    }
}