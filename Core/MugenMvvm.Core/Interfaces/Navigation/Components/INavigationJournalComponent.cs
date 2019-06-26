using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationJournalComponent : IComponent<INavigationDispatcher>
    {
        IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext? metadata = null);

        INavigationEntry? GetNavigationEntryById(string navigationOperationId, IReadOnlyMetadataContext? metadata = null);

        INavigationEntry? GetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null);
    }
}