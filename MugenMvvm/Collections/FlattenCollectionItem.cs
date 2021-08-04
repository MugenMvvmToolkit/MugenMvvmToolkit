using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal sealed class FlattenCollectionItem<T> : FlattenCollectionItemBase, ICollectionChangedListener<T>
    {
        [Preserve(Conditional = true)]
        public FlattenCollectionItem()
        {
        }

        public void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            using var _ = BatchIfNeed();
            OnAdded((IReadOnlyObservableCollection) collection, item, index);
        }

        public void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            using var _ = BatchIfNeed();
            OnReplaced((IReadOnlyObservableCollection) collection, oldItem, newItem, index);
        }

        public void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            using var _ = BatchIfNeed();
            OnMoved((IReadOnlyObservableCollection) collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            using var _ = BatchIfNeed();
            OnRemoved((IReadOnlyObservableCollection) collection, item, index);
        }

        public void OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items) => OnReset(collection, AsObjectEnumerable(items));

        protected override IEnumerable<object?> GetItems() => Collection.AsEnumerable();

        private static IEnumerable<object?>? AsObjectEnumerable(IEnumerable<T>? items) => items == null ? null : items as IEnumerable<object?> ?? items.Cast<object>();

        private ActionToken BatchIfNeed()
        {
            if (TryGetDecoratorManager(out var _, out var decorator) && Indexes.Count > 1)
                return decorator.BatchUpdate();
            return default;
        }
    }
}