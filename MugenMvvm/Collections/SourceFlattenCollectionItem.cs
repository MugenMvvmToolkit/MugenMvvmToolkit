using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal sealed class SourceFlattenCollectionItem<TItem> : FlattenCollectionItemBase, ICollectionChangedListener<TItem>
    {
        public void OnAdded(IReadOnlyObservableCollection<TItem> collection, TItem item, int index)
        {
            using var _ = BatchIfNeed();
            OnAdded((IReadOnlyObservableCollection) collection, item, index);
        }

        public void OnReplaced(IReadOnlyObservableCollection<TItem> collection, TItem oldItem, TItem newItem, int index)
        {
            using var _ = BatchIfNeed();
            OnReplaced((IReadOnlyObservableCollection) collection, oldItem, newItem, index);
        }

        public void OnMoved(IReadOnlyObservableCollection<TItem> collection, TItem item, int oldIndex, int newIndex)
        {
            using var _ = BatchIfNeed();
            OnMoved((IReadOnlyObservableCollection) collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection<TItem> collection, TItem item, int index)
        {
            using var _ = BatchIfNeed();
            OnRemoved((IReadOnlyObservableCollection) collection, item, index);
        }

        public void OnReset(IReadOnlyObservableCollection<TItem> collection, IEnumerable<TItem>? items) => OnReset(collection, AsObjectEnumerable(items));

        protected override IEnumerable<object?> GetItems() => Collection.AsEnumerable();

        private static IEnumerable<object?>? AsObjectEnumerable(IEnumerable<TItem>? items) => items == null ? null : items as IEnumerable<object?> ?? items.Cast<object>();

        private ActionToken BatchIfNeed() => DecoratorManager != null && Indexes.Count > 1 ? DecoratorManager.BatchUpdate(Decorator.Owner, Decorator) : default;
    }
}