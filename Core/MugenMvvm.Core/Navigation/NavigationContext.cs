using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationContext : INavigationContext
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public NavigationContext(INavigationProvider navigationProvider, NavigationType navigationType, string navigationOperationId,
            NavigationMode navigationMode, IMetadataContextProvider? metadataContextProvider, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationType = navigationType;
            NavigationOperationId = navigationOperationId;
            NavigationProvider = navigationProvider;
            NavigationMode = navigationMode;
            _metadataContextProvider = metadataContextProvider;
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata is IMetadataContext ctx)
                    return ctx;
                return _metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);
            }
        }

        public NavigationMode NavigationMode { get; }

        public NavigationType NavigationType { get; }

        public string NavigationOperationId { get; }

        public INavigationProvider NavigationProvider { get; }

        #endregion
    }
}