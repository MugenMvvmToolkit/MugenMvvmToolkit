using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationJournalListener : IComponent<INavigationJournalComponent>
    {
        void OnNavigationEntryAdded(INavigationJournalComponent navigationJournal, INavigationEntry navigationEntry);

        void OnNavigationEntryUpdated(INavigationJournalComponent navigationJournal, INavigationEntry navigationEntry);

        void OnNavigationEntryRemoved(INavigationJournalComponent navigationJournal, INavigationEntry navigationEntry);
    }
}