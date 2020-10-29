using System.Collections;
using Android.Views;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Bindings;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public class CollectionViewManager : ICollectionViewManager
    {
        #region Implementation of interfaces

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
            var providerType = ViewGroupExtensions.GetItemSourceProviderType(target);
            if (providerType == ViewGroupExtensions.NoneProviderType)
                BindingExceptionManager.ThrowInvalidBindingMember(target, BindableMembers.For<View>().ItemsSource());
            var itemTemplateSelector = target.BindableMembers().ItemTemplateSelector();
            if (itemTemplateSelector == null)
                ExceptionManager.ThrowObjectNotInitialized(target, target.BindableMembers().Descriptor.ItemTemplateSelector());

            var itemsSourceProvider = ViewGroupExtensions.GetItemsSourceProvider(target);
            var hasFragments = itemTemplateSelector is IFragmentTemplateSelector fts && fts.HasFragments;
            if (providerType == ViewGroupExtensions.ContentRawProviderType)
                ContentItemsSourceGenerator.GetOrAdd(target, (IContentTemplateSelector) itemTemplateSelector).Collection = value;
            else if (providerType == ViewGroupExtensions.ContentProviderType || providerType == ViewGroupExtensions.ResourceOrContentProviderType && hasFragments)
            {
                if (!(itemsSourceProvider is ContentItemsSourceProvider provider))
                {
                    ViewExtensions.RemoveParentObserver(target);
                    provider = new ContentItemsSourceProvider(target, (IContentTemplateSelector) itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    ViewGroupExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.ItemsSource = value;
            }
            else
            {
                if (!(itemsSourceProvider is ResourceItemsSourceProvider provider))
                {
                    ViewExtensions.RemoveParentObserver(target);
                    provider = new ResourceItemsSourceProvider(target, (IResourceTemplateSelector) itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    ViewGroupExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.ItemsSource = value;
            }
        }

        public virtual object? GetSelectedItem(object collectionView)
        {
            if (!(collectionView is View target) || !ViewGroupExtensions.IsSelectedIndexSupported(target))
            {
                BindingExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<View>().SelectedItem());
                return null;
            }

            var index = ViewGroupExtensions.GetSelectedIndex(target);
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

            if (!ViewGroupExtensions.SetSelectedIndex(target, index))
                BindingExceptionManager.ThrowInvalidBindingMember(collectionView, BindableMembers.For<View>().SelectedItem());
        }

        public virtual void ReloadItem(object collectionView, object? item)
        {
        }

        #endregion

        #region Methods

        protected static object? GetItemsSourceProvider(View target)
        {
            var itemsSourceProvider = ViewGroupExtensions.GetItemsSourceProvider(target);
            if (itemsSourceProvider != null)
                return itemsSourceProvider;
            return ContentItemsSourceGenerator.TryGet(target);
        }

        #endregion
    }
}