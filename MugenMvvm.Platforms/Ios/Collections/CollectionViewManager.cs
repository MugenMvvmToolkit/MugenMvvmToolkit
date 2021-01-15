using System.Collections;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Ios.Interfaces;
using MugenMvvm.Ios.Members;
using UIKit;

namespace MugenMvvm.Ios.Collections
{
    public class CollectionViewManager : ICollectionViewManager
    {
        protected static ItemsSourceBindableCollectionAdapter? GetCollectionAdapter(object collectionView, bool throwIfNotSupported)
        {
            if (collectionView is UITableView tableView)
                return ((MugenTableViewSource) tableView.Source)?.CollectionAdapter;
            if (collectionView is UICollectionView collection)
                return ((MugenCollectionViewSource) collection.Source)?.CollectionAdapter;
            if (throwIfNotSupported)
                ExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().ItemsSource());
            return null;
        }

        protected static ICellTemplateSelector GetCellTemplateSelector(UIView view)
        {
            var itemTemplateSelector = (ICellTemplateSelector?) view.BindableMembers().ItemTemplateSelector();
            if (itemTemplateSelector == null)
                ExceptionManager.ThrowObjectNotInitialized(view, view.BindableMembers().Descriptor.ItemTemplateSelector());
            itemTemplateSelector.Initialize(view);
            return itemTemplateSelector;
        }

        public virtual IEnumerable? GetItemsSource(object collectionView) => GetCollectionAdapter(collectionView, true)?.Collection;

        public virtual void SetItemsSource(object collectionView, IEnumerable? value)
        {
            if (collectionView is UITableView tableView)
            {
                if (tableView.Source is not MugenTableViewSource source)
                {
                    source = new MugenTableViewSource(tableView, GetCellTemplateSelector(tableView));
                    tableView.Source = source;
                }

                source.CollectionAdapter.Collection = value;
                return;
            }

            if (collectionView is UICollectionView collection)
            {
                if (collection.Source is not MugenCollectionViewSource source)
                {
                    source = new MugenCollectionViewSource(collection, GetCellTemplateSelector(collection));
                    collection.Source = source;
                }

                source.CollectionAdapter.Collection = value;
                return;
            }

            ExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().ItemsSource());
        }

        public virtual object? GetSelectedItem(object collectionView)
        {
            ExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().SelectedItem());
            return null;
        }

        public virtual void SetSelectedItem(object collectionView, object? value) =>
            ExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().SelectedItem());

        public virtual void ReloadItem(object collectionView, object? item) => GetCollectionAdapter(collectionView, false)?.Reload(item);
    }
}