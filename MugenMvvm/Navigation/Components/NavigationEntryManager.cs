using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryManager : INavigationEntryProviderComponent, INavigationDispatcherNavigatingListener, IHasPriority
    {
        #region Fields

        private readonly Dictionary<NavigationType, List<INavigationEntry>> _navigationEntries;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationEntryManager()
        {
            _navigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.EntryProvider;

        #endregion

        #region Implementation of interfaces

        public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            INavigationEntry? addedEntry = null;
            INavigationEntry? updatedEntry = null;
            INavigationEntry? removedEntry = null;
            lock (_navigationEntries)
            {
                if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsNew)
                {
                    if (!_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        list = new List<INavigationEntry>();
                        _navigationEntries[navigationContext.NavigationType] = list;
                    }

                    updatedEntry = FindEntry(list, navigationContext.NavigationId);
                    if (updatedEntry == null)
                    {
                        addedEntry = new NavigationEntry(navigationContext.Target, navigationContext.NavigationProvider, navigationContext.NavigationId,
                            navigationContext.NavigationType, navigationContext.GetMetadataOrDefault().ToNonReadonly());
                        list.Add(addedEntry);
                    }
                }

                if (navigationContext.NavigationMode.IsClose)
                {
                    if (_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        removedEntry = FindEntry(list, navigationContext.NavigationId);
                        if (removedEntry != null)
                            list.Remove(removedEntry);
                    }
                }
            }

            if (addedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(navigationContext.GetMetadataOrDefault())
                    .OnNavigationEntryAdded(navigationDispatcher, addedEntry, navigationContext);
            }
            else if (updatedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(navigationContext.GetMetadataOrDefault())
                    .OnNavigationEntryUpdated(navigationDispatcher, updatedEntry, navigationContext);
            }
            else if (removedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(navigationContext.GetMetadataOrDefault())
                    .OnNavigationEntryRemoved(navigationDispatcher, removedEntry, navigationContext);
            }
        }

        public ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> TryGetNavigationEntries(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata)
        {
            var result = ItemOrListEditor.Get<INavigationEntry>();
            lock (_navigationEntries)
            {
                foreach (var t in _navigationEntries)
                    result.AddRange(ItemOrList.FromList(t.Value));
            }

            return result.ToItemOrList<IReadOnlyList<INavigationEntry>>();
        }

        #endregion

        #region Methods

        private static INavigationEntry? FindEntry(List<INavigationEntry> entries, string id)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].NavigationId == id)
                    return entries[i];
            }

            return null;
        }

        #endregion
    }
}