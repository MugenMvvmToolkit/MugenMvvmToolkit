using System.Collections;
using Android.Views;
using MugenMvvm.Android.Bindings;
using MugenMvvm.Android.Collections;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views.Components;
using IViewManager = MugenMvvm.Interfaces.Views.IViewManager;

namespace MugenMvvm.Android.Views
{
    public sealed class ViewCollectionManager : IViewCollectionManagerComponent, IHasPriority
    {
        public int Priority { get; init; } = ViewComponentPriority.ViewCollectionManager;

        public bool TryGetItemsSource(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            if (view is IMenu menu)
            {
                itemsSource = MenuItemsSourceGenerator.TryGet(menu);
                return true;
            }

            if (view is not View v)
            {
                itemsSource = null;
                return false;
            }

            itemsSource = GetItemsSourceProvider(v);
            return true;
        }

        public bool TryGetItemsSourceRaw(IViewManager viewManager, object view, IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            if (view is IMenu menu)
            {
                itemsSource = MenuItemsSourceGenerator.TryGet(menu)?.Collection;
                return true;
            }

            if (view is not View v)
            {
                itemsSource = null;
                return false;
            }

            itemsSource = GetItemsSourceProvider(v)?.Collection;
            return true;
        }

        public bool TrySetItemsSource(IViewManager viewManager, object view, IEnumerable? value, IReadOnlyMetadataContext? metadata)
        {
            if (view is IMenu menu)
            {
                MenuItemsSourceGenerator.GetOrAdd(menu).Collection = value;
                return true;
            }

            if (view is not View target)
                return false;

            var providerType = NativeBindableMemberMugenExtensions.GetItemSourceProviderType(target);
            if (providerType == ItemSourceProviderType.None)
                return false;

            var itemTemplateSelector = target.BindableMembers().ItemTemplateSelector();
            if (itemTemplateSelector == null)
                ExceptionManager.ThrowObjectNotInitialized(target, target.BindableMembers().Descriptor.ItemTemplateSelector());

            var itemsSourceProvider = NativeBindableMemberMugenExtensions.GetItemsSourceProvider(target);
            var hasFragments = itemTemplateSelector is IFragmentTemplateSelector fts && fts.HasFragments;
            if (providerType == ItemSourceProviderType.ContentRaw)
                ContentItemsSourceGenerator.GetOrAdd(target, (IContentTemplateSelector)itemTemplateSelector).Collection = value;
            else if (providerType == ItemSourceProviderType.Content ||
                     providerType == ItemSourceProviderType.ResourceOrContent && hasFragments)
            {
                if (itemsSourceProvider is not ContentItemsSourceProvider provider)
                {
                    ViewMugenExtensions.RemoveParentObserver(target);
                    provider = new ContentItemsSourceProvider(target, (IContentTemplateSelector)itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    NativeBindableMemberMugenExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.CollectionAdapter.Collection = value;
            }
            else
            {
                if (itemsSourceProvider is not ResourceItemsSourceProvider provider)
                {
                    ViewMugenExtensions.RemoveParentObserver(target);
                    provider = new ResourceItemsSourceProvider(target, (IResourceTemplateSelector)itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    NativeBindableMemberMugenExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.CollectionAdapter.Collection = value;
            }

            return true;
        }

        private static BindableCollectionAdapter? GetItemsSourceProvider(View target)
        {
            var itemsSourceProvider = (IItemsSourceProvider?)NativeBindableMemberMugenExtensions.GetItemsSourceProvider(target);
            if (itemsSourceProvider != null)
                return itemsSourceProvider.CollectionAdapter;
            return ContentItemsSourceGenerator.TryGet(target);
        }
    }
}