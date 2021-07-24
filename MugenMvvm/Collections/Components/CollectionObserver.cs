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

        protected override IEnumerable<object?>? GetItems() => OwnerOptional?.AsEnumerable();

        void ICollectionChangedListener<object?>.OnAdded(IReadOnlyObservableCollection<object?> collection, object? item, int index) => OnAdded(item);

        void ICollectionChangedListener<object?>.OnReplaced(IReadOnlyObservableCollection<object?> collection, object? oldItem, object? newItem, int index) =>
            OnReplaced(oldItem, newItem);

        void ICollectionChangedListener<object?>.OnMoved(IReadOnlyObservableCollection<object?> collection, object? item, int oldIndex, int newIndex) => OnMoved(item);

        void ICollectionChangedListener<object?>.OnRemoved(IReadOnlyObservableCollection<object?> collection, object? item, int index) => OnRemoved(item);

        void ICollectionChangedListener<object?>.OnReset(IReadOnlyObservableCollection<object?> collection, IEnumerable<object?>? items) => OnReset(null, items);
    }
}