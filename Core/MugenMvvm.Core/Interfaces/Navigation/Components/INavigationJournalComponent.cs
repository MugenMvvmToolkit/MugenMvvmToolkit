using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    //todo check componenet design move to INavigationDispatcher?
    public interface INavigationJournalComponent : IComponent<INavigationDispatcher>, IComponentOwner<INavigationJournalComponent>
    {
        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata);

        INavigationEntry? GetNavigationEntryById(string navigationOperationId, IReadOnlyMetadataContext metadata);

        INavigationEntry? GetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext metadata);
    }
}