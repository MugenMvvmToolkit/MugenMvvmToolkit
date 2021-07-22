using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections
{
    internal sealed class DecoratorFlattenCollectionItem : FlattenCollectionItemBase, ICollectionDecoratorListener, ICollectionBatchUpdateListener
    {
        public DecoratorFlattenCollectionItem(IEnumerable collection, FlattenCollectionDecorator decorator) : base(collection, decorator)
        {
        }

        protected override IEnumerable<object?> GetItems() => Collection.DecoratedItems();
    }
}