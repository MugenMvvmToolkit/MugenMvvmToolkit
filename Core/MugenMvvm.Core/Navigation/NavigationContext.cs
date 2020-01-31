using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationContext : MetadataOwnerBase, INavigationContext
    {
        #region Constructors

        public NavigationContext(INavigationProvider navigationProvider, NavigationType navigationType, string navigationOperationId,
            NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationType = navigationType;
            NavigationOperationId = navigationOperationId;
            NavigationProvider = navigationProvider;
            NavigationMode = navigationMode;
        }

        #endregion

        #region Properties

        public NavigationMode NavigationMode { get; }

        public NavigationType NavigationType { get; }

        public string NavigationOperationId { get; }

        public INavigationProvider NavigationProvider { get; }

        #endregion
    }
}