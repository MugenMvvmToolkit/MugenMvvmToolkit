using System.Collections;
using Android.Views;
using MugenMvvm.Android.Bindings;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public class CollectionViewManager : ICollectionViewManager
    {
        protected static object? GetItemsSourceProvider(View target)
        {
            var itemsSourceProvider = NativeBindableMemberMugenExtensions.GetItemsSourceProvider(target);
            if (itemsSourceProvider != null)
                return itemsSourceProvider;
            return ContentItemsSourceGenerator.TryGet(target);
        }

        public virtual IEnumerable? GetItemsSource(object collectionView)
        {
            if (collectionView is IMenu menu)
                return MenuItemsSourceGenerator.TryGet(menu)?.Collection;

            var itemsSourceProvider = GetItemsSourceProvider((View) collectionView);
            if (itemsSourceProvider == null)
                return null;
            if (itemsSourceProvider is BindableCollectionAdapter adapter)
                return adapter.Collection;
            return ((IItemsSourceProvider) itemsSourceProvider).ItemsSource;
        }

        public virtual void SetItemsSource(object collectionView, IEnumerable? value)
        {
            if (collectionView is IMenu menu)
            {
                MenuItemsSourceGenerator.GetOrAdd(menu).Collection = value;
                return;
            }

            var target = (View) collectionView;
            var providerType = NativeBindableMemberMugenExtensions.GetItemSourceProviderType(target);
            if (providerType == ItemSourceProviderType.None)
                ExceptionManager.ThrowInvalidBindingMember(target, nameof(ViewBindableMembers.ItemsSource));
            var itemTemplateSelector = target.BindableMembers().ItemTemplateSelector();
            if (itemTemplateSelector == null)
                ExceptionManager.ThrowObjectNotInitialized(target, target.BindableMembers().Descriptor.ItemTemplateSelector());

            var itemsSourceProvider = NativeBindableMemberMugenExtensions.GetItemsSourceProvider(target);
            var hasFragments = itemTemplateSelector is IFragmentTemplateSelector fts && fts.HasFragments;
            if (providerType == ItemSourceProviderType.ContentRaw)
                ContentItemsSourceGenerator.GetOrAdd(target, (IContentTemplateSelector) itemTemplateSelector).Collection = value;
            else if (providerType == ItemSourceProviderType.Content ||
                     providerType == ItemSourceProviderType.ResourceOrContent && hasFragments)
            {
                if (itemsSourceProvider is not ContentItemsSourceProvider provider)
                {
                    ViewMugenExtensions.RemoveParentObserver(target);
                    provider = new ContentItemsSourceProvider(target, (IContentTemplateSelector) itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    NativeBindableMemberMugenExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.ItemsSource = value;
            }
            else
            {
                if (itemsSourceProvider is not ResourceItemsSourceProvider provider)
                {
                    ViewMugenExtensions.RemoveParentObserver(target);
                    provider = new ResourceItemsSourceProvider(target, (IResourceTemplateSelector) itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    NativeBindableMemberMugenExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.ItemsSource = value;
            }
        }

        public virtual object? GetSelectedItem(object collectionView)
        {
            if (collectionView is not View target || !NativeBindableMemberMugenExtensions.IsSelectedIndexSupported(target))
            {
                ExceptionManager.ThrowInvalidBindingMember(collectionView, nameof(ViewBindableMembers.SelectedItem));
                return null;
            }

            var index = NativeBindableMemberMugenExtensions.GetSelectedIndex(target);
            if (index < 0)
                return null;
            var itemsSourceProvider = GetItemsSourceProvider(target);
            if (itemsSourceProvider == null)
                return null;
            if (itemsSourceProvider is IItemsSourceProvider p)
                return p.GetItemAt(index);
            return ((BindableCollectionAdapter) itemsSourceProvider)[index];
        }

        public virtual void SetSelectedItem(object collectionView, object? value)
        {
            var target = (View) collectionView;
            int index;
            var itemsSourceProvider = GetItemsSourceProvider(target);
            if (itemsSourceProvider == null)
                index = -1;
            else
            {
                if (itemsSourceProvider is IItemsSourceProvider p)
                    index = p.IndexOf(value);
                else
                    index = ((BindableCollectionAdapter) itemsSourceProvider).IndexOf(value);
            }

            if (!NativeBindableMemberMugenExtensions.SetSelectedIndex(target, index, target.BindableMembers().SmoothScroll()))
                ExceptionManager.ThrowInvalidBindingMember(collectionView, nameof(ViewBindableMembers.SelectedItem));
        }

        public virtual void ReloadItem(object collectionView, object? item)
        {
        }
    }
}