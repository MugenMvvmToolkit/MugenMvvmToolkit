﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    internal sealed class FlattenCollectionItem<TItem> : FlattenCollectionItemBase, ICollectionChangedListener<TItem>
    {
        [Preserve(Conditional = true)]
        public FlattenCollectionItem()
        {
        }

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

        private ActionToken BatchIfNeed()
        {
            if (TryGetDecoratorManager(out var decoratorManager, out var decorator) && Indexes.Count > 1)
                return decoratorManager.BatchUpdate(decorator.Owner, decorator);
            return default;
        }
    }
}