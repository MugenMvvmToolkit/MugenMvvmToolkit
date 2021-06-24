using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Collections.Components
{
    public class ItemObserverCollectionDecoratorListener : ItemObserverCollectionListenerBase<object?>, ICollectionDecoratorListener
    {
        public ItemObserverCollectionDecoratorListener() : base(null)
        {
        }

        public ItemObserverCollectionDecoratorListener(IEqualityComparer<object?>? comparer) : base(comparer)
        {
        }

        void ICollectionDecoratorListener.OnChanged(IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
        }

        void ICollectionDecoratorListener.OnAdded(IReadOnlyObservableCollection collection, object? item, int index) => OnAdded(item);

        void ICollectionDecoratorListener.OnReplaced(IReadOnlyObservableCollection collection, object? oldItem, object? newItem, int index) => OnReplaced(oldItem, newItem);

        void ICollectionDecoratorListener.OnMoved(IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex) => OnMoved(item);

        void ICollectionDecoratorListener.OnRemoved(IReadOnlyObservableCollection collection, object? item, int index) => OnRemoved(item);

        void ICollectionDecoratorListener.OnReset(IReadOnlyObservableCollection collection, IEnumerable<object?>? items) => OnReset(null, items);
    }
}