using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationEntry : INavigationEntry
    {
        #region Constructors

        public NavigationEntry(INavigationProvider navigationProvider, string navigationOperationId, NavigationType navigationType, IMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            NavigationType = navigationType;
            NavigationOperationId = navigationOperationId;
            NavigationProvider = navigationProvider;
            Metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => Metadata.Count != 0;

        public IMetadataContext Metadata { get; }

        public string NavigationOperationId { get; }

        public NavigationType NavigationType { get; }

        public INavigationProvider NavigationProvider { get; }

        #endregion
    }
}