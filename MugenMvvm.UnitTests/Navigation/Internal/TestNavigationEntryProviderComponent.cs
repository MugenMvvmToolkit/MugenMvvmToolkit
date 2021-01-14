using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationEntryProviderComponent : INavigationEntryProviderComponent, IHasPriority
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        public TestNavigationEntryProviderComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public Func<IReadOnlyMetadataContext?, ItemOrIReadOnlyList<INavigationEntry>>? TryGetNavigationEntries { get; set; }

        public int Priority { get; set; }

        ItemOrIReadOnlyList<INavigationEntry> INavigationEntryProviderComponent.TryGetNavigationEntries(INavigationDispatcher navigationDispatcher,
            IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryGetNavigationEntries?.Invoke(metadata) ?? default;
        }
    }
}