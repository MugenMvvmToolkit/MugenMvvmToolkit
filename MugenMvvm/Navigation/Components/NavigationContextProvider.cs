﻿using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationContextProvider : INavigationContextProviderComponent, IHasPriority
    {
        public int Priority { get; init; } = NavigationComponentPriority.ContextProvider;

        public INavigationContext TryGetNavigationContext(INavigationDispatcher navigationDispatcher, object? target, INavigationProvider navigationProvider, string navigationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null) =>
            new NavigationContext(target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
    }
}