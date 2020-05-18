using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryProviderComponent : IComponent<INavigationDispatcher>
    {
        IReadOnlyList<INavigationEntry>? TryGetNavigationEntries(IReadOnlyMetadataContext? metadata);
    }
}