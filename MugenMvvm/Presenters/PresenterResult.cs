using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters
{
    public sealed class PresenterResult : MetadataOwnerBase, IPresenterResult
    {
        public PresenterResult(object? target, string navigationId, INavigationProvider navigationProvider, NavigationType navigationType,
            IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNullOrEmpty(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Target = target;
            NavigationId = navigationId;
            NavigationProvider = navigationProvider;
            NavigationType = navigationType;
        }

        public string NavigationId { get; }

        public NavigationType NavigationType { get; }

        public INavigationProvider NavigationProvider { get; }

        public object? Target { get; }
    }
}