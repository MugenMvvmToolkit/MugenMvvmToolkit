using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryProviderComponent : IComponent<INavigationDispatcher>
    {
        ItemOrIReadOnlyList<INavigationEntry> TryGetNavigationEntries(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata);
    }
}