using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherJournalListener : IListener
    {
        bool? CanAddNavigationEntry(INavigationDispatcherJournal navigationDispatcherJournal, INavigationContext navigationContext);

        bool? CanRemoveNavigationEntry(INavigationDispatcherJournal navigationDispatcherJournal, INavigationContext navigationContext);

        IReadOnlyList<INavigationCallback> GetCallbacks(INavigationDispatcherJournal navigationDispatcherJournal, INavigationEntry navigationEntry,
            NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);

        void OnNavigationEntriesUpdated(INavigationDispatcherJournal navigationDispatcherJournal, IReadOnlyDictionary<NavigationType, List<INavigationEntry>> oldEntries,
            IReadOnlyDictionary<NavigationType, List<INavigationEntry>> newEntries, IReadOnlyMetadataContext metadata);
    }
}