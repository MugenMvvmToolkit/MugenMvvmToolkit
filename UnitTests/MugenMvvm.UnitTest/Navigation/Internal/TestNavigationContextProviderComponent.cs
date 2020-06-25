using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationContextProviderComponent : INavigationContextProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object?, INavigationProvider, string, NavigationType, NavigationMode, IReadOnlyMetadataContext?, INavigationContext?>? TryGetNavigationContext { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        INavigationContext? INavigationContextProviderComponent.TryGetNavigationContext(object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata)
        {
            return TryGetNavigationContext?.Invoke(target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
        }

        #endregion
    }
}