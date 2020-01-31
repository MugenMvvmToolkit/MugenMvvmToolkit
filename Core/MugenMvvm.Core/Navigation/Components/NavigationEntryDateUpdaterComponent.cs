using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryDateUpdaterComponent : INavigationDispatcherEntryListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.EntryUpdater;

        #endregion

        #region Implementation of interfaces

        public void OnNavigationEntryAdded(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            navigationEntry.Metadata.Set(NavigationMetadata.NavigationDate, DateTime.UtcNow);
        }

        public void OnNavigationEntryUpdated(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            navigationEntry.Metadata.Set(NavigationMetadata.NavigationDate, DateTime.UtcNow);
        }

        public void OnNavigationEntryRemoved(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
        }

        #endregion
    }
}