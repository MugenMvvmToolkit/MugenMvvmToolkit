using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryFinderComponent : IComponent<INavigationJournalComponent>
    {
        INavigationEntry? TryGetGetNavigationEntryById(IEnumerable<INavigationEntry> entries, string navigationOperationId, IReadOnlyMetadataContext metadata);

        INavigationEntry? TryGetPreviousNavigationEntry(IEnumerable<INavigationEntry> entries, INavigationEntry navigationEntry, IReadOnlyMetadataContext metadata);
    }
}