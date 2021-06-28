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
    public class MugenTableViewSource : UITableViewSource
    {
        private const int InitializedStateMask = 1;

        public MugenTableViewSource(UITableView tableView, ICellTemplateSelector itemTemplateSelector)
            : this(new ItemsSourceBindableCollectionAdapter(new TableViewCollectionAdapter(tableView), itemTemplateSelector as IDiffableEqualityComparer), itemTemplateSelector)
        {
        }

        public MugenTableViewSource(ItemsSourceBindableCollectionAdapter collectionAdapter, ICellTemplateSelector itemTemplateSelector)
        {
            Should.NotBeNull(collectionAdapter, nameof(collectionAdapter));
            Should.NotBeNull(itemTemplateSelector, nameof(itemTemplateSelector));
            CollectionAdapter = collectionAdapter;
            ItemTemplateSelector = itemTemplateSelector;
        }

        public ItemsSourceBindableCollectionAdapter CollectionAdapter { get; }

        public ICellTemplateSelector ItemTemplateSelector { get; }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = tableView.DequeueReusableCell(ItemTemplateSelector.GetIdentifier(tableView, item), indexPath);
            cell.BindableMembers().SetDataContext(item);
            if ((cell.Tag & InitializedStateMask) != InitializedStateMask)
            {
                (cell as ISupportInitialize)?.BeginInit();
                cell.Tag |= InitializedStateMask;
                cell.BindableMembers().SetParent(tableView);
                ItemTemplateSelector.OnCellCreated(tableView, cell);
                (cell as ISupportInitialize)?.EndInit();
            }

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section) => CollectionAdapter.Count;

        protected virtual object? GetItemAt(NSIndexPath indexPath) => CollectionAdapter[indexPath.Row];
    }
}