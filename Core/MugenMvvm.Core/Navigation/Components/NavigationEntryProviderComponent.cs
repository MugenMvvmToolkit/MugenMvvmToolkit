using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryProviderComponent : INavigationEntryProviderComponent, INavigationDispatcherNavigatedListener, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly Dictionary<NavigationType, List<INavigationEntry>> _navigationEntries;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationEntryProviderComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _navigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.EntryProvider;

        #endregion

        #region Implementation of interfaces

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var components = navigationDispatcher.GetComponents();
            INavigationEntry? addedEntry = null;
            INavigationEntry? updatedEntry = null;
            INavigationEntry? removedEntry = null;
            lock (_navigationEntries)
            {
                if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsBack || navigationContext.NavigationMode.IsNew)
                {
                    if (!_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        list = new List<INavigationEntry>();
                        _navigationEntries[navigationContext.NavigationType] = list;
                    }

                    updatedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                    if (updatedEntry == null)
                    {
                        addedEntry = new NavigationEntry(navigationContext.NavigationProvider, navigationContext.NavigationOperationId,
                            navigationContext.NavigationType, _metadataContextProvider.DefaultIfNull().GetMetadataContext(this, navigationContext.Metadata));
                        list.Add(addedEntry);
                    }
                }
                else if (navigationContext.NavigationMode.IsClose)
                {
                    if (_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        removedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                        if (removedEntry != null)
                            list.Remove(removedEntry);
                    }
                }
            }

            if (addedEntry != null)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as INavigationDispatcherEntryListener)?.OnNavigationEntryAdded(navigationDispatcher, addedEntry, navigationContext);
            }
            else if (updatedEntry != null)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as INavigationDispatcherEntryListener)?.OnNavigationEntryUpdated(navigationDispatcher, updatedEntry, navigationContext);
            }
            else if (removedEntry != null)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as INavigationDispatcherEntryListener)?.OnNavigationEntryRemoved(navigationDispatcher, removedEntry, navigationContext);
            }
        }

        public IReadOnlyList<INavigationEntry> TryGetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext? metadata)
        {
            List<INavigationEntry>? result = null;
            lock (_navigationEntries)
            {
                if (type == null)
                {
                    foreach (var t in _navigationEntries)
                        AddNavigationEntries(t.Key, ref result);
                }
                else
                    AddNavigationEntries(type, ref result);
            }

            return result ?? (IReadOnlyList<INavigationEntry>)Default.EmptyArray<INavigationEntry>();
        }

        #endregion

        #region Methods

        private void AddNavigationEntries(NavigationType type, ref List<INavigationEntry>? result)
        {
            if (_navigationEntries.TryGetValue(type, out var list))
            {
                if (result == null)
                    result = list.ToList();
                else
                    result.AddRange(list);
            }
        }

        private static INavigationEntry? FindEntry(List<INavigationEntry> entries, string id)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].NavigationOperationId == id)
                    return entries[i];
            }

            return null;
        }

        #endregion
    }
}