using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryProviderComponent : IComponent<INavigationDispatcher>
    {
        INavigationEntry? TryGetNavigationEntry(INavigationContext navigationContext);
    }
}