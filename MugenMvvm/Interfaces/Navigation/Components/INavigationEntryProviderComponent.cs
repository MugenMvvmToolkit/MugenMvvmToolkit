using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryProviderComponent : IComponent<INavigationDispatcher>
    {
        ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> TryGetNavigationEntries(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata);
    }
}