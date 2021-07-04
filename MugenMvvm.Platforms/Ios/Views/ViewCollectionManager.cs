using System.Collections;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Ios.Bindings;
using MugenMvvm.Ios.Collections;
using MugenMvvm.Ios.Interfaces;
using UIKit;

namespace MugenMvvm.Ios.Views
{
    public sealed class ViewCollectionManager : IViewCollectionManagerComponent, IHasPriority
    {
        public int Priority { get; init; } = ViewComponentPriority.ViewCollectionManager;

        public bool TryGetItemsSource(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            if (view is not UIView v)
            {
                itemsSource = null;
                return false;
            }

            itemsSource = GetCollectionAdapter(v);
            return true;
        }

        public bool TryGetItemsSourceRaw(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            if (view is not UIView v)
            {
                itemsSource = null;
                return false;
            }

            itemsSource = GetCollectionAdapter(v)?.Collection;
            return true;
        }

        public bool TrySetItemsSource(IViewManager viewManager, object view, IEnumerable? value, IReadOnlyMetadataContext? metadata)
        {
            if (view is UITableView tableView)
            {
                if (tableView.Source is not MugenTableViewSource source)
                {
                    source = new MugenTableViewSource(tableView, GetCellTemplateSelector(tableView));
                    tableView.Source = source;
                }

                source.CollectionAdapter.Collection = value;
                return true;
            }

            if (view is UICollectionView collection)
            {
                if (collection.Source is not MugenCollectionViewSource source)
                {
                    source = new MugenCollectionViewSource(collection, GetCellTemplateSelector(collection));
                    collection.Source = source;
                }

                source.CollectionAdapter.Collection = value;
                return true;
            }

            return false;
        }

        private static ItemsSourceBindableCollectionAdapter? GetCollectionAdapter(object collectionView)
        {
            if (collectionView is UITableView tableView)
                return ((MugenTableViewSource)tableView.Source)?.CollectionAdapter;
            if (collectionView is UICollectionView collection)
                return ((MugenCollectionViewSource)collection.Source)?.CollectionAdapter;
            return null;
        }

        private static ICellTemplateSelector GetCellTemplateSelector(UIView view)
        {
            var itemTemplateSelector = (ICellTemplateSelector?)view.BindableMembers().ItemTemplateSelector();
            if (itemTemplateSelector == null)
                ExceptionManager.ThrowObjectNotInitialized(view, view.BindableMembers().Descriptor.ItemTemplateSelector());
            itemTemplateSelector.Initialize(view);
            return itemTemplateSelector;
        }
    }
}