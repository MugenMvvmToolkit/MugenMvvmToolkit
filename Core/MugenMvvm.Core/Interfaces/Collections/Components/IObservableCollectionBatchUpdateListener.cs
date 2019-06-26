using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IObservableCollectionBatchUpdateListener<T> : IComponent<IObservableCollection<T>>
    {
        bool IsDecoratorComponent { get; }

        void OnBeginBatchUpdate(IObservableCollection<T> collection);

        void OnEndBatchUpdate(IObservableCollection<T> collection);
    }
}