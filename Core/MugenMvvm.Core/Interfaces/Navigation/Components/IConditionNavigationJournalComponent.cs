using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface IConditionNavigationJournalComponent : IComponent<INavigationDispatcher>
    {
        bool CanAddNavigationEntry(INavigationContext navigationContext);

        bool CanRemoveNavigationEntry(INavigationContext navigationContext);
    }
}