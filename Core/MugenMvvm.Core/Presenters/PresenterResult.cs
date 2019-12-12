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
        public readonly string NavigationOperationId;
        public readonly INavigationProvider NavigationProvider;
        public readonly NavigationType NavigationType;

        #endregion

        #region Constructors

        public PresenterResult(string navigationOperationId, INavigationProvider navigationProvider, NavigationType navigationType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNullOrEmpty(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationOperationId = navigationOperationId;
            NavigationProvider = navigationProvider;
            NavigationType = navigationType;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool IsEmpty => NavigationProvider == null;

        #endregion
    }
}