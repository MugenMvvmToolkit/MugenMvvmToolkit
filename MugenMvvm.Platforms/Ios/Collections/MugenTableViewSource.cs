using System;
using Foundation;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Ios.Interfaces;
using UIKit;

namespace MugenMvvm.Ios.Collections
{
    public class MugenTableViewSource : UITableViewSource
    {
        #region Fields

        private const int InitializedStateMask = 1;

        #endregion

        #region Constructors

        public MugenTableViewSource(UITableView tableView, ICellTemplateSelector itemTemplateSelector)
            : this(new ItemsSourceBindableCollectionAdapter(new TableViewCollectionAdapter(tableView), itemTemplateSelector as IItemsSourceEqualityComparer), itemTemplateSelector)
        {
        }

        public MugenTableViewSource(ItemsSourceBindableCollectionAdapter collectionAdapter, ICellTemplateSelector itemTemplateSelector)
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

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = tableView.DequeueReusableCell(ItemTemplateSelector.GetIdentifier(tableView, item), indexPath);
            cell.BindableMembers().SetDataContext(item);
            if ((cell.Tag & InitializedStateMask) == InitializedStateMask)
            {
                cell.Tag |= InitializedStateMask;
                cell.BindableMembers().SetParent(tableView);
                ItemTemplateSelector.OnCellCreated(tableView, cell);
            }

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section) => CollectionAdapter.Count;

        protected virtual object? GetItemAt(NSIndexPath indexPath) => CollectionAdapter[indexPath.Row];

        #endregion
    }
}