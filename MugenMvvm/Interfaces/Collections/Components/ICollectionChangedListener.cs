using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionChangedListener : ICollectionChangedListenerBase, IComponent<IObservableCollection>
    {
    }
}