using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherJournal : IHasListeners<INavigationDispatcherJournalListener>, IAttachableComponent<INavigationDispatcher>
    {
        void OnNavigated(INavigationContext navigationContext);

        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata);
    }
}