using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionBatchUpdateListener : IComponent<IObservableCollection>
    {
        void OnBeginBatchUpdate(IObservableCollection collection);

        void OnEndBatchUpdate(IObservableCollection collection);
    }
}