using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    public class CollectionObserver : CollectionObserverBase, ICollectionChangedListener<object?>
    {
        [Preserve(Conditional = true)]
        public CollectionObserver()
        {
        }

        protected internal override IEnumerable<object?> GetItems(IReadOnlyObservableCollection collection) => collection.AsEnumerable();

        void ICollectionChangedListener<object?>.OnAdded(IReadOnlyObservableCollection<object?> collection, object? item, int index) => OnAdded(collection, item);

        void ICollectionChangedListener<object?>.OnReplaced(IReadOnlyObservableCollection<object?> collection, object? oldItem, object? newItem, int index) =>
            OnReplaced(collection, oldItem, newItem);

        void ICollectionChangedListener<object?>.OnMoved(IReadOnlyObservableCollection<object?> collection, object? item, int oldIndex, int newIndex) => OnMoved(collection, item);

        void ICollectionChangedListener<object?>.OnRemoved(IReadOnlyObservableCollection<object?> collection, object? item, int index) => OnRemoved(collection, item);

        void ICollectionChangedListener<object?>.OnReset(IReadOnlyObservableCollection<object?> collection, IEnumerable<object?>? items) => OnReset(collection, null, items);
    }
}