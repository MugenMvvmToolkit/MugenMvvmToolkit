using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationContextProvider : INavigationContextProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.ContextProvider;

        #endregion

        #region Implementation of interfaces

        public INavigationContext TryGetNavigationContext(INavigationDispatcher navigationDispatcher, object? target, INavigationProvider navigationProvider, string navigationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            return new NavigationContext(target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
        }

        #endregion
    }
}