using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollection<T> : IComponentOwner<IObservableCollection<T>>, IList<T>
    {
        IObservableCollectionDecoratorManager<T> DecoratorManager { get; }

        IEnumerable<T> DecorateItems();

        ActionToken BeginBatchUpdate(BatchUpdateCollectionMode mode = BatchUpdateCollectionMode.Both);

        void Move(int oldIndex, int newIndex);

        void Reset(IEnumerable<T> items);

        void RaiseItemChanged(T item, object? args);
    }
}