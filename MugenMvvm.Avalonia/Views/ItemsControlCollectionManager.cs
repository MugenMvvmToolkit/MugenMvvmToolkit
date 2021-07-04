using System.Collections;
using Avalonia.Controls;
using MugenMvvm.Avalonia.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Avalonia.Views
{
    public sealed class ItemsControlCollectionManager : IViewCollectionManagerComponent, IHasPriority
    {
        public int Priority { get; init; } = ViewComponentPriority.ViewCollectionManager;

        public bool TryGetItemsSource(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            if (view is ItemsControl itemsControl)
            {
                itemsSource = itemsControl.Items;
                return true;
            }

            itemsSource = null;
            return false;
        }

        public bool TryGetItemsSourceRaw(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            if (view is ItemsControl itemsControl)
            {
                itemsSource = ObservableCollectionAdapter.GetItemsSource(itemsControl.Items);
                return true;
            }

            itemsSource = null;
            return false;
        }

        public bool TrySetItemsSource(IViewManager viewManager, object view, IEnumerable? value, IReadOnlyMetadataContext? metadata)
        {
            if (view is ItemsControl itemsControl)
            {
                GetOrAdd(itemsControl).Adapter.Collection = value;
                return true;
            }

            return false;
        }

        private static ObservableCollectionAdapter GetOrAdd(ItemsControl target)
        {
            if (target.Items is not ObservableCollectionAdapter c)
                c = new ObservableCollectionAdapter(target);
            return c;
        }
    }
}