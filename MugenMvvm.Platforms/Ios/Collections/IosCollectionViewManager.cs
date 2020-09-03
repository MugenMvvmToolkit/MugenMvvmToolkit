using System.Collections;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Ios.Interfaces;
using MugenMvvm.Ios.Members;
using UIKit;

namespace MugenMvvm.Ios.Collections
{
    public class IosCollectionViewManager : ICollectionViewManager
    {
        #region Implementation of interfaces

        public virtual IEnumerable? GetItemsSource(object collectionView) => GetCollectionAdapter(collectionView, true)?.Collection;

        public virtual void SetItemsSource(object collectionView, IEnumerable? value)
        {
            if (collectionView is UITableView tableView)
            {
                if (!(tableView.Source is MugenTableViewSource source))
                {
                    source = new MugenTableViewSource(tableView, GetCellTemplateSelector(tableView));
                    tableView.Source = source;
                }

                source.CollectionAdapter.Collection = value;
                return;
            }

            if (collectionView is UICollectionView collection)
            {
                if (!(collection.Source is MugenCollectionViewSource source))
                {
                    source = new MugenCollectionViewSource(collection, GetCellTemplateSelector(collection));
                    collection.Source = source;
                }

                source.CollectionAdapter.Collection = value;
                return;
            }

            BindingExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().ItemsSource());
        }

        public virtual object? GetSelectedItem(object collectionView)
        {
            BindingExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().SelectedItem());
            return null;
        }

        public virtual void SetSelectedItem(object collectionView, object? value) => BindingExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().SelectedItem());

        public virtual void ReloadItem(object collectionView, object? item) => GetCollectionAdapter(collectionView, false)?.Reload(item);

        #endregion

        #region Methods

        protected static IosBindableCollectionAdapter? GetCollectionAdapter(object collectionView, bool throwIfNotSupported)
        {
            if (collectionView is UITableView tableView)
                return ((MugenTableViewSource) tableView.Source)?.CollectionAdapter;
            if (collectionView is UICollectionView collection)
                return ((MugenCollectionViewSource) collection.Source)?.CollectionAdapter;
            if (throwIfNotSupported)
                BindingExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<UIView>().ItemsSource());
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

        #endregion
    }
}