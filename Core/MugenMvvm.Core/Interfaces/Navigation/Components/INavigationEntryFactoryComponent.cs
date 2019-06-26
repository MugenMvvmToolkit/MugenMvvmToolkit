using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryFactoryComponent : IComponent<INavigationDispatcher>
    {
        INavigationEntry? TryGetNavigationEntry(INavigationContext navigationContext);
    }
}