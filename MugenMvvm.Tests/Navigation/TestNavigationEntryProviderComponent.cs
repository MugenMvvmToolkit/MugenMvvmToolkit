using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationEntryProviderComponent : INavigationEntryProviderComponent, IHasPriority
    {
        public Func<INavigationDispatcher, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<INavigationEntry>>? TryGetNavigationEntries { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<INavigationEntry> INavigationEntryProviderComponent.TryGetNavigationEntries(INavigationDispatcher navigationDispatcher,
            IReadOnlyMetadataContext? metadata) =>
            TryGetNavigationEntries?.Invoke(navigationDispatcher, metadata) ?? default;
    }
}