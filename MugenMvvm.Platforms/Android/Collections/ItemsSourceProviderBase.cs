using System.Collections;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Android.Collections
{
    public abstract class ItemsSourceProviderBase<TSelector> : Object, IItemsSourceProvider where TSelector : class
    {
        protected ItemsSourceProviderBase(object owner, TSelector itemTemplateSelector, IStableIdProvider? stableIdProvider,
            ItemsSourceBindableCollectionAdapter? collectionAdapter)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(itemTemplateSelector, nameof(itemTemplateSelector));
            Owner = owner;
            ItemTemplateSelector = itemTemplateSelector;
            StableIdProvider = stableIdProvider ?? itemTemplateSelector as IStableIdProvider;
            CollectionAdapter = collectionAdapter ??
                                new ItemsSourceBindableCollectionAdapter(itemTemplateSelector as IDiffableEqualityComparer ?? stableIdProvider as IDiffableEqualityComparer);
        }

        public object Owner { get; }

        public TSelector ItemTemplateSelector { get; }

        public IStableIdProvider? StableIdProvider { get; }

        public ItemsSourceBindableCollectionAdapter CollectionAdapter { get; }

        public virtual int Count => CollectionAdapter.Count;

        public virtual bool HasStableId => StableIdProvider != null;

        public virtual object? GetItemAt(int position) => CollectionAdapter[position];

        public virtual int IndexOf(object? item) => CollectionAdapter.IndexOf(item);

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

        public virtual ICharSequence GetItemTitleFormatted(int position) => (ItemTemplateSelector as ITitleTemplateSelector)?.TryGetTitle(Owner, GetItemAt(position))!;

        public virtual void Swap(int indexA, int indexB)
        {
            if (CollectionAdapter.Collection is IList list)
            {
                var tmp = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = tmp;
            }
        }

        public virtual void Move(int oldIndex, int newIndex)
        {
            if (CollectionAdapter.Collection is IList list && oldIndex != newIndex)
            {
                var obj = list[oldIndex];
                list.RemoveAt(oldIndex);
                list.Insert(newIndex, obj);
            }
        }

        public virtual void RemoveAt(int index) => (CollectionAdapter.Collection as IList)?.RemoveAt(index);

        public virtual void AddObserver(IItemsSourceObserver observer) => CollectionAdapter.AddObserver(observer);

        public virtual void RemoveObserver(IItemsSourceObserver observer) => CollectionAdapter.RemoveObserver(observer);
    }
}