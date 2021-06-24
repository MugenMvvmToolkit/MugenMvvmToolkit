using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionItemPreInitializerComponent<in T> : IComponent<IReadOnlyObservableCollection>
    {
        void Initialize(IReadOnlyObservableCollection<T> collection, T item);
    }
}