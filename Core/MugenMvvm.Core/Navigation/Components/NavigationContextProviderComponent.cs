using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationContextProviderComponent : INavigationContextProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationContextProviderComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority => int.MaxValue;

        #endregion

        #region Implementation of interfaces

        public INavigationContext GetNavigationContext(INavigationProvider navigationProvider, string navigationOperationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            return new NavigationContext(navigationProvider, navigationType, navigationOperationId, navigationMode, _metadataContextProvider, metadata);
        }

        #endregion

        #region Nested types

        private sealed class NavigationContext : INavigationContext
        {
            #region Fields

            private readonly IMetadataContextProvider? _metadataContextProvider;
            private IReadOnlyMetadataContext? _metadata;

            #endregion

            #region Constructors

            public NavigationContext(INavigationProvider navigationProvider, NavigationType navigationType, string navigationOperationId,
                NavigationMode navigationMode, IMetadataContextProvider? metadataContextProvider, IReadOnlyMetadataContext? metadata)
            {
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

        #endregion
    }
}