using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Presenters
{
    public sealed class PresenterResult : IPresenterResult
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public PresenterResult(INavigationProvider navigationProvider, string navigationOperationId, NavigationType navigationType,
            IMetadataContextProvider? metadataContextProvider, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            _metadataContextProvider = metadataContextProvider;
            _metadata = metadata;
            NavigationProvider = navigationProvider;
            NavigationOperationId = navigationOperationId;
            NavigationType = navigationType;
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

        public string NavigationOperationId { get; }

        public INavigationProvider NavigationProvider { get; }

        public NavigationType NavigationType { get; }

        #endregion
    }
}