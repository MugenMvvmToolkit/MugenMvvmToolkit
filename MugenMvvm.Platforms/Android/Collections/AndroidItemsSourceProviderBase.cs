using System.Collections;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;

namespace MugenMvvm.Android.Collections
{
    public abstract class AndroidItemsSourceProviderBase<TSelector> : Object, IAndroidItemsSourceProvider where TSelector : class
    {
        #region Constructors

        protected AndroidItemsSourceProviderBase(object owner, TSelector itemTemplateSelector, IStableIdProvider? stableIdProvider, AndroidBindableCollectionAdapter? collectionAdapter)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(itemTemplateSelector, nameof(itemTemplateSelector));
            Owner = owner;
            ItemTemplateSelector = itemTemplateSelector;
            StableIdProvider = stableIdProvider ?? itemTemplateSelector as IStableIdProvider;
            CollectionAdapter = collectionAdapter ?? new AndroidBindableCollectionAdapter(itemTemplateSelector as IItemSourceEqualityComparer ?? stableIdProvider as IItemSourceEqualityComparer);
        }

        #endregion

        #region Properties

        public object Owner { get; }

        public AndroidBindableCollectionAdapter CollectionAdapter { get; }

        public TSelector ItemTemplateSelector { get; }

        public IStableIdProvider? StableIdProvider { get; }

        public virtual int Count => CollectionAdapter.Count;

        public virtual bool HasStableId => StableIdProvider != null;

        public virtual IEnumerable? ItemsSource
        {
            get => CollectionAdapter.Collection;
            set => CollectionAdapter.Collection = value;
        }

        #endregion

        #region Implementation of interfaces

        public virtual long GetItemId(int position)
        {
            if (StableIdProvider == null)
                return position;
            return StableIdProvider.GetId(GetItemAt(position));
        }

        public virtual bool ContainsItem(long itemId)
        {
            if (StableIdProvider == null)
                return false;

            for (var i = 0; i < CollectionAdapter.Count; i++)
            {
                if (StableIdProvider.GetId(CollectionAdapter[i]) == itemId)
                    return true;
            }

            return false;
        }

        public virtual ICharSequence GetItemTitleFormatted(int position) => (ItemTemplateSelector as ITitleTemplateSelector)?.GetTitle(Owner, GetItemAt(position))!;

        public virtual void AddObserver(IItemsSourceObserver observer) => CollectionAdapter.AddObserver(observer);

        public virtual void RemoveObserver(IItemsSourceObserver observer) => CollectionAdapter.RemoveObserver(observer);

        public virtual object? GetItemAt(int position) => CollectionAdapter[position];

        public virtual int IndexOf(object? item) => CollectionAdapter.IndexOf(item);

        #endregion
    }
}