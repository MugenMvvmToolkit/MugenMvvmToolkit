﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationEntry : MetadataOwnerBase, INavigationEntry
    {
        #region Constructors

        public NavigationEntry(object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType,
            IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
            : base(metadata, metadataContextManager)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Target = target;
            NavigationType = navigationType;
            NavigationId = navigationId;
            NavigationProvider = navigationProvider;
        }

        #endregion

        #region Properties

        public string NavigationId { get; }

        public NavigationType NavigationType { get; }

        public INavigationProvider NavigationProvider { get; }

        public object? Target { get; }

        #endregion
    }
}