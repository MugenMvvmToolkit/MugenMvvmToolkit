using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    internal sealed class DecoratedCollectionSynchronizer : CollectionSynchronizerBase<object?>, ICollectionDecoratorListener
    {
        public DecoratedCollectionSynchronizer(IList<object?> target) : base(target, BatchUpdateType.Decorators)
        {
        }

        public void OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            if (GetTarget(collection) is IReadOnlyObservableCollection observableCollection)
                observableCollection.RaiseItemChanged(item, args);
        }
    }
}