using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Collections.Components
{
    public interface ICollectionDecoratorListener : ICollectionChangedListenerBase, IComponent<IObservableCollection>
    {
    }
}