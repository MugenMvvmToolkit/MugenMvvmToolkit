using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Presenters
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct PresenterResult
    {
        #region Fields

        public readonly IReadOnlyMetadataContext Metadata;
        public readonly string NavigationId;
        public readonly INavigationProvider NavigationProvider;
        public readonly NavigationType NavigationType;

        #endregion

        #region Constructors

        public PresenterResult(string navigationId, INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNullOrEmpty(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationId = navigationId;
            NavigationProvider = navigationProvider;
            NavigationType = navigationType;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool IsEmpty => NavigationProvider == null;

        #endregion

        #region Methods

        public PresenterResult UpdateMetadata(IReadOnlyMetadataContext? metadata)
        {
            if (IsEmpty)
                return default;
            return new PresenterResult(NavigationId, NavigationProvider, NavigationType, metadata);
        }

        #endregion
    }
}