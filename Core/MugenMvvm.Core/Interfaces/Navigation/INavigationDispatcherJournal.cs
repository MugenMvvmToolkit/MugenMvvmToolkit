using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherJournal : IHasListeners<INavigationDispatcherJournalListener>
    {
        void Initialize(INavigationDispatcher navigationDispatcher);

        void OnNavigated(INavigationContext navigationContext);

        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata);
    }
}