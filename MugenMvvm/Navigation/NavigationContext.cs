﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationContext : MetadataOwnerBase, INavigationContext
    {
        public NavigationContext(object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Target = target;
            NavigationMode = navigationMode;
            NavigationType = navigationType;
            NavigationId = navigationId;
            NavigationProvider = navigationProvider;
        }

        public NavigationType NavigationType { get; }

        public string NavigationId { get; }

        public INavigationProvider NavigationProvider { get; }

        public object? Target { get; }

        public NavigationMode NavigationMode { get; }
    }
}