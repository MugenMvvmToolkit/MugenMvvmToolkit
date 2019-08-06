using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationContextProviderComponent : INavigationContextProviderComponent
    {
        #region Fields

        private readonly IMetadataContextProvider _metadataContextProvider;

        #endregion

        #region Constructors

        public NavigationContextProviderComponent(IMetadataContextProvider metadataContextProvider)
        {
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = int.MaxValue;

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            return Priority;
        }

        public INavigationContext GetNavigationContext(INavigationProvider navigationProvider, string navigationOperationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            return new NavigationContext(navigationProvider, navigationType, navigationOperationId, navigationMode, metadata, _metadataContextProvider);
        }

        #endregion

        #region Nested types

        private sealed class NavigationContext : INavigationContext
        {
            #region Fields

            private readonly IMetadataContextProvider _metadataContextProvider;
            private IMetadataContext? _metadata;

            #endregion

            #region Constructors

            public NavigationContext(INavigationProvider navigationProvider, NavigationType navigationType, string navigationOperationId,
                NavigationMode navigationMode, IReadOnlyMetadataContext? metadata, IMetadataContextProvider metadataContextProvider)
            {
                _metadataContextProvider = metadataContextProvider;
                NavigationType = navigationType;
                NavigationOperationId = navigationOperationId;
                NavigationProvider = navigationProvider;
                NavigationMode = navigationMode;
                if (metadata != null)
                    _metadata = metadata.ToNonReadonly(this, metadataContextProvider);
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        _metadataContextProvider.LazyInitialize(ref _metadata, this);
                    return _metadata;
                }
            }

            public NavigationMode NavigationMode { get; }

            public NavigationType NavigationType { get; }

            public string NavigationOperationId { get; }

            public INavigationProvider NavigationProvider { get; }

            #endregion
        }

        #endregion
    }
}