using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryFactoryComponent : IComponent<INavigationJournalComponent>
    {
        INavigationEntry? TryGetNavigationEntry(INavigationContext navigationContext);
    }
}