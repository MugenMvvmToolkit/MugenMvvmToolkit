using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationEntry : MetadataOwnerBase, INavigationEntry
    {
        public NavigationEntry(object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType,
            IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Target = target;
            NavigationType = navigationType;
            NavigationId = navigationId;
            NavigationProvider = navigationProvider;
        }

        public string NavigationId { get; }

        public NavigationType NavigationType { get; }

        public INavigationProvider NavigationProvider { get; }

        public object? Target { get; }

        public bool IsPending { get; set; }
    }
}