using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface IPreInitializerCollectionComponent<in T> : IComponent<IReadOnlyObservableCollection>
    {
        void Initialize(IReadOnlyObservableCollection<T> collection, T item);
    }
}