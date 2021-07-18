using System;
using System.ComponentModel;
using Foundation;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Ios.Interfaces;
using UIKit;

namespace MugenMvvm.Ios.Collections
{
    public class MugenCollectionViewSource : UICollectionViewSource
    {
        private const int InitializedStateMask = 1;

        public MugenCollectionViewSource(UICollectionView collectionView, ICellTemplateSelector itemTemplateSelector)
            : this(new ItemsSourceBindableCollectionAdapter(new CollectionViewAdapter(collectionView), itemTemplateSelector as IDiffableEqualityComparer), itemTemplateSelector)
        {
        }

        public MugenCollectionViewSource(ItemsSourceBindableCollectionAdapter collectionAdapter, ICellTemplateSelector itemTemplateSelector)
        {
            Should.NotBeNull(collectionAdapter, nameof(collectionAdapter));
            Should.NotBeNull(itemTemplateSelector, nameof(itemTemplateSelector));
            CollectionAdapter = collectionAdapter;
            ItemTemplateSelector = itemTemplateSelector;
        }

        public ItemsSourceBindableCollectionAdapter CollectionAdapter { get; }

        public ICellTemplateSelector ItemTemplateSelector { get; }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var identifier = ItemTemplateSelector.TryGetIdentifier(collectionView, item);
            if (ReferenceEquals(identifier, null))
                ExceptionManager.ThrowTemplateNotSupported(collectionView, item);

            var cell = (UICollectionViewCell) collectionView.DequeueReusableCell(identifier, indexPath);
            cell.BindableMembers().SetDataContext(item);
            if ((cell.Tag & InitializedStateMask) != InitializedStateMask)
            {
                (cell as ISupportInitialize)?.BeginInit();
                cell.Tag |= InitializedStateMask;
                cell.BindableMembers().SetParent(collectionView);
                ItemTemplateSelector.OnCellCreated(collectionView, cell);
                (cell as ISupportInitialize)?.EndInit();
            }

            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section) => CollectionAdapter.Count;

        protected virtual object? GetItemAt(NSIndexPath indexPath) => CollectionAdapter[indexPath.Row];
    }
}