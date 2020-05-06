using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationEntryFinderComponent : INavigationEntryFinderComponent, IHasPriority
    {
        #region Properties

        public Func<INavigationEntry, IReadOnlyMetadataContext?, INavigationEntry?>? TryGetPreviousNavigationEntry { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        INavigationEntry? INavigationEntryFinderComponent.TryGetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata)
        {
            return TryGetPreviousNavigationEntry?.Invoke(navigationEntry, metadata);
        }

        #endregion
    }
}