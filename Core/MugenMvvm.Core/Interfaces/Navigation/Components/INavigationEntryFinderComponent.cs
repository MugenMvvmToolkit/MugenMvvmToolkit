using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryFinderComponent : IComponent<INavigationDispatcher>
    {
        INavigationEntry? TryGetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata);
    }
}