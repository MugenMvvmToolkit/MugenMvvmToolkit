using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation
{
    public class TestNavigationEntryProviderComponent : INavigationEntryProviderComponent, IHasPriority
    {
        #region Properties

        public Func<NavigationType?, IReadOnlyMetadataContext?, IReadOnlyList<INavigationEntry>?>? TryGetNavigationEntries { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<INavigationEntry>? INavigationEntryProviderComponent.TryGetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext? metadata)
        {
            return TryGetNavigationEntries?.Invoke(type, metadata);
        }

        #endregion
    }
}