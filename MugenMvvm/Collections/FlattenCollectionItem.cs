using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections
{
    internal sealed class FlattenCollectionItem<T> : FlattenCollectionItemBase, ICollectionChangedListener<T>
    {
        [Preserve(Conditional = true)]
        public FlattenCollectionItem()
        {
        }

        public void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAdded((IReadOnlyObservableCollection)collection, item, index);

        public void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) =>
            OnReplaced((IReadOnlyObservableCollection)collection, oldItem, newItem, index);

        public void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) =>
            OnMoved((IReadOnlyObservableCollection)collection, item, oldIndex, newIndex);

        public void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) => OnRemoved((IReadOnlyObservableCollection)collection, item, index);

        public void OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items) => OnReset(collection, AsObjectEnumerable(items));

        protected override IEnumerable<object?> GetItems() => Collection.AsEnumerable();

        private static IEnumerable<object?>? AsObjectEnumerable(IEnumerable<T>? items) => items == null ? null : items as IEnumerable<object?> ?? items.Cast<object>();
    }
}