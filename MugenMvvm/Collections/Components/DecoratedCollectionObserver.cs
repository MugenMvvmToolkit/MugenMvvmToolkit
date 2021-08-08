using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public class DecoratedCollectionObserver : CollectionObserverBase, IListenerCollectionDecorator, IEnumerable<object?>
    {
        [Preserve(Conditional = true)]
        public DecoratedCollectionObserver()
        {
        }

        public bool IsLazy => false;

        bool ICollectionDecorator.HasAdditionalItems => false;

        public IEnumerator<object?> GetEnumerator() => OwnerOptional.DecoratedItems<object?>(this)?.GetEnumerator() ?? Default.EmptyEnumerator<object?>();

        protected internal override IEnumerable<object?> GetItems(IReadOnlyObservableCollection collection) => this;

        bool ICollectionDecorator.TryGetIndexes(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object? item, ref ItemOrListEditor<int> indexes) => false;

        IEnumerable<object?> ICollectionDecorator.Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items) => items;

        bool ICollectionDecorator.OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args) => true;

        bool ICollectionDecorator.OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            OnAdded(collection, item);
            return true;
        }

        bool ICollectionDecorator.OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            OnReplaced(collection, oldItem, newItem);
            return true;
        }

        bool ICollectionDecorator.OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            OnMoved(collection, item);
            return true;
        }

        bool ICollectionDecorator.OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            OnRemoved(collection, item);
            return true;
        }

        bool ICollectionDecorator.OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            OnReset(collection, null, items);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}