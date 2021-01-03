using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryDateTracker : INavigationEntryListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.EntryTracker;

        #endregion

        #region Implementation of interfaces

        public void OnNavigationEntryAdded(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo) =>
            navigationEntry.Metadata.Set(NavigationMetadata.NavigationDate, DateTime.UtcNow, out _);

        public void OnNavigationEntryUpdated(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo) =>
            navigationEntry.Metadata.Set(NavigationMetadata.NavigationDate, DateTime.UtcNow, out _);

        public void OnNavigationEntryRemoved(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo)
        {
        }

        #endregion
    }
}