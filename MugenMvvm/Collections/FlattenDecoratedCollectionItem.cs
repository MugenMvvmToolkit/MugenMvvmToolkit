using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections
{
    internal sealed class FlattenDecoratedCollectionItem : FlattenCollectionItemBase, IDecoratedCollectionChangedListener, ICollectionBatchUpdateListener
    {
        public FlattenDecoratedCollectionItem(object item, IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak) : base(item, collection, decorator, isWeak)
        {
        }

        protected internal override IEnumerable<object?> GetItems() => Collection.DecoratedItems();
    }
}