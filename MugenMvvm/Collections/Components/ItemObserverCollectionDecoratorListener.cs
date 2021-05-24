using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    public class ItemObserverCollectionDecoratorListener : ItemObserverCollectionListenerBase<object?>, ICollectionDecoratorListener
    {
        public ItemObserverCollectionDecoratorListener() : base(null)
        {
        }

        public ItemObserverCollectionDecoratorListener(IEqualityComparer<object?>? comparer) : base(comparer)
        {
        }

        protected override void OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            CollectionDecoratorManager.GetOrAdd((IEnumerable) owner);
        }

        void ICollectionDecoratorListener.OnChanged(ICollection collection, object? item, int index, object? args)
        {
        }

        void ICollectionDecoratorListener.OnAdded(ICollection collection, object? item, int index) => OnAdded(item);

        void ICollectionDecoratorListener.OnReplaced(ICollection collection, object? oldItem, object? newItem, int index) => OnReplaced(oldItem, newItem);

        void ICollectionDecoratorListener.OnMoved(ICollection collection, object? item, int oldIndex, int newIndex) => OnMoved(item);

        void ICollectionDecoratorListener.OnRemoved(ICollection collection, object? item, int index) => OnRemoved(item);

        void ICollectionDecoratorListener.OnReset(ICollection collection, IEnumerable<object?>? items) => OnReset(null, items);
    }
}