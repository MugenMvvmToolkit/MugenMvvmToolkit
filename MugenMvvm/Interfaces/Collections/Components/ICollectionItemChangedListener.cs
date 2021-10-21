using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionItemChangedListener : IComponent<IReadOnlyObservableCollection>
    {
        void OnChanged(IReadOnlyObservableCollection collection, object? item, object? args);
    }
}