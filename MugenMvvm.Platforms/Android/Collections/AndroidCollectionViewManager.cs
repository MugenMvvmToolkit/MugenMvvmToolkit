using System.Collections;
using Android.Views;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Members;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Collections
{
    public class AndroidCollectionViewManager : ICollectionViewManager
    {
        #region Implementation of interfaces

        public virtual IEnumerable? GetItemsSource(object collectionView)
        {
            if (collectionView is IMenu menu)
                return AndroidMenuItemsSourceGenerator.TryGet(menu)?.Collection;

            var itemsSourceProvider = GetItemsSourceProvider((View) collectionView);
            if (itemsSourceProvider == null)
                return null;
            if (itemsSourceProvider is BindableCollectionAdapter adapter)
                return adapter.Collection;
            return ((IAndroidItemsSourceProvider) itemsSourceProvider).ItemsSource;
        }

        public virtual void SetItemsSource(object collectionView, IEnumerable? value)
        {
            if (collectionView is IMenu menu)
            {
                AndroidMenuItemsSourceGenerator.GetOrAdd(menu).Collection = value;
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
                AndroidContentItemsSourceGenerator.GetOrAdd(target, (IContentTemplateSelector) itemTemplateSelector).Collection = value;
            else if (providerType == ViewGroupExtensions.ContentProviderType || providerType == ViewGroupExtensions.ResourceOrContentProviderType && hasFragments)
            {
                if (!(itemsSourceProvider is AndroidContentItemsSourceProvider provider))
                {
                    ViewExtensions.RemoveParentObserver(target);
                    provider = new AndroidContentItemsSourceProvider(target, (IContentTemplateSelector) itemTemplateSelector, target.BindableMembers().StableIdProvider());
                    ViewGroupExtensions.SetItemsSourceProvider(target, provider, hasFragments);
                }

                provider.ItemsSource = value;
            }
            else
            {
                if (!(itemsSourceProvider is AndroidResourceItemsSourceProvider provider))
                {
                    ViewExtensions.RemoveParentObserver(target);
                    provider = new AndroidResourceItemsSourceProvider(target, (IResourceTemplateSelector) itemTemplateSelector, target.BindableMembers().StableIdProvider());
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
            if (itemsSourceProvider is IAndroidItemsSourceProvider p)
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
                if (itemsSourceProvider is IAndroidItemsSourceProvider p)
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
            return AndroidContentItemsSourceGenerator.TryGet(target);
        }

        #endregion
    }
}