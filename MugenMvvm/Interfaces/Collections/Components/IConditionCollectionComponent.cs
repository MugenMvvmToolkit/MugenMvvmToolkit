using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IConditionCollectionComponent : IComponent<IObservableCollection>
    {
        bool CanAdd<T>(IObservableCollection<T> collection, T item, int index);

        bool CanReplace<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index);

        bool CanMove<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex);

        bool CanRemove<T>(IObservableCollection<T> collection, T item, int index);

        bool CanReset<T>(IObservableCollection<T> collection, IEnumerable<T> items);

        bool CanClear<T>(IObservableCollection<T> collection);
    }
}