using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections
{
    internal sealed class FlattenDecoratedCollectionItem : FlattenCollectionItemBase, ICollectionDecoratorListener, ICollectionBatchUpdateListener
    {
        public FlattenDecoratedCollectionItem(IEnumerable collection, FlattenCollectionDecorator decorator, bool isWeak) : base(collection, decorator, isWeak)
        {
        }

        protected override IEnumerable<object?> GetItems() => Collection.DecoratedItems();
    }
}