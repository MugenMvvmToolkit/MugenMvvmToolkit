using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface IConditionNavigationJournalComponent : IComponent<INavigationJournalComponent>
    {
        bool CanAddNavigationEntry(INavigationContext navigationContext);

        bool CanRemoveNavigationEntry(INavigationContext navigationContext);
    }
}