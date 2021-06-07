using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationContextProviderComponent : INavigationContextProviderComponent, IHasPriority
    {
        public Func<INavigationDispatcher, object?, INavigationProvider, string, NavigationType, NavigationMode, IReadOnlyMetadataContext?, INavigationContext?>?
            TryGetNavigationContext { get; set; }

        public int Priority { get; set; }

        INavigationContext? INavigationContextProviderComponent.TryGetNavigationContext(INavigationDispatcher navigationDispatcher, object? target,
            INavigationProvider navigationProvider, string navigationId,
            NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata) =>
            TryGetNavigationContext?.Invoke(navigationDispatcher, target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
    }
}