using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationEntry : MetadataOwnerBase, INavigationEntry
    {
        #region Constructors

        public NavigationEntry(INavigationProvider navigationProvider, string navigationOperationId, NavigationType navigationType,
            IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationType = navigationType;
            NavigationOperationId = navigationOperationId;
            NavigationProvider = navigationProvider;
        }

        #endregion

        #region Properties

        public string NavigationOperationId { get; }

        public NavigationType NavigationType { get; }

        public INavigationProvider NavigationProvider { get; }

        #endregion
    }
}