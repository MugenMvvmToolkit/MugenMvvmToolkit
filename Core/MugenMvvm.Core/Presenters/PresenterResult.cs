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
        #region Constructors

        public PresenterResult(INavigationProvider navigationProvider, string navigationOperationId, NavigationType navigationType,
            IReadOnlyMetadataContext? metadata, IMetadataContextProvider? contextProvider)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationProvider = navigationProvider;
            NavigationOperationId = navigationOperationId;
            NavigationType = navigationType;
            Metadata = metadata.ToNonReadonly(navigationProvider, contextProvider);
        }

        #endregion

        #region Properties

        public bool HasMetadata => true;

        public IMetadataContext Metadata { get; }

        public string NavigationOperationId { get; }

        public INavigationProvider NavigationProvider { get; }

        public NavigationType NavigationType { get; }

        #endregion

        #region Methods

        public static IPresenterResult ViewModelResult(INavigationProvider provider, NavigationType navigationType, IViewModelBase viewModel,
            IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? contextProvider = null)
        {
            return ViewModelResult(provider, provider.GetUniqueNavigationOperationId(viewModel), navigationType, viewModel, metadata, contextProvider);
        }

        public static IPresenterResult ViewModelResult(INavigationProvider provider, string navigationOperationId, NavigationType navigationType, IViewModelBase viewModel,
            IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? contextProvider = null)
        {
            var result = new PresenterResult(provider, navigationOperationId, navigationType, metadata, contextProvider);
            result.Metadata.Set(NavigationMetadata.ViewModel, viewModel);
            return result;
        }

        #endregion
    }
}