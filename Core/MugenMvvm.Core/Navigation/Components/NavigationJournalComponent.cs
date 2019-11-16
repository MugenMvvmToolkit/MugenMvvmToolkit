using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationJournalComponent : AttachableComponentBase<INavigationDispatcher>, INavigationJournalComponent, INavigationDispatcherNavigatedListener,
        IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly Dictionary<NavigationType, List<INavigationEntry>> _navigationEntries;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationJournalComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _navigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = int.MaxValue;

        #endregion

        #region Implementation of interfaces

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var components = Owner.GetComponents();
            INavigationEntry? addedEntry = null;
            INavigationEntry? updatedEntry = null;
            INavigationEntry? removedEntry = null;
            lock (_navigationEntries)
            {
                if (CanAddNavigationEntry(components, navigationContext))
                {
                    if (!_navigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        list = new List<INavigationEntry>();
                        _navigationEntries[navigationContext.NavigationType] = list;
                    }

                    updatedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                    if (updatedEntry == null)
                    {
                        addedEntry = GetNavigationEntry(components, navigationContext);
                        list.Add(addedEntry);
                    }
                    else if (updatedEntry is NavigationEntry entry)
                        entry.UpdateNavigationDate();
                }
                else if (CanRemoveNavigationEntry(components, navigationContext))
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
                    (components[i] as INavigationJournalListener)?.OnNavigationEntryAdded(this, addedEntry);
            }
            else if (updatedEntry != null)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as INavigationJournalListener)?.OnNavigationEntryUpdated(this, updatedEntry);
            }
            else if (removedEntry != null)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as INavigationJournalListener)?.OnNavigationEntryRemoved(this, removedEntry);
            }
        }

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext? metadata = null)
        {
            lock (_navigationEntries)
            {
                List<INavigationEntry>? result = null;
                if (type == null)
                {
                    foreach (var t in _navigationEntries)
                        AddNavigationEntries(t.Key, ref result);
                }
                else
                    AddNavigationEntries(type, ref result);

                if (result == null)
                    return Default.EmptyArray<INavigationEntry>();
                return result;
            }
        }

        public INavigationEntry? GetNavigationEntryById(string navigationOperationId, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            IEnumerable<INavigationEntry>? entries = null;
            lock (_navigationEntries)
            {
                var components = Owner.GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    var navigationEntry = (components[i] as INavigationEntryFinderComponent)
                        ?.TryGetGetNavigationEntryById(entries ??= _navigationEntries.SelectMany(pair => pair.Value), navigationOperationId, metadata);
                    if (navigationEntry != null)
                        return navigationEntry;
                }

                foreach (var navigationEntry in _navigationEntries)
                {
                    var findEntry = FindEntry(navigationEntry.Value, navigationOperationId);
                    if (findEntry != null)
                        return findEntry;
                }

                return null;
            }
        }

        public INavigationEntry? GetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            IEnumerable<INavigationEntry>? entries = null;
            lock (_navigationEntries)
            {
                var components = Owner.GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    var result = (components[i] as INavigationEntryFinderComponent)
                        ?.TryGetPreviousNavigationEntry(entries ??= _navigationEntries.SelectMany(pair => pair.Value), navigationEntry, metadata);
                    if (result != null)
                        return result;
                }

                if (navigationEntry.NavigationType.IsUndefined)
                    return null;

                if (navigationEntry.NavigationType.IsNestedNavigation)
                {
                    if (!_navigationEntries.TryGetValue(navigationEntry.NavigationType, out var list))
                        return null;
                    return list
                        .Where(entry => entry.NavigationProvider.Id == navigationEntry.NavigationProvider.Id)
                        .OrderByDescending(entry => entry.NavigationDate)
                        .FirstOrDefault();
                }

                if (navigationEntry.NavigationType.IsSystemNavigation || !_navigationEntries.TryGetValue(navigationEntry.NavigationType, out var navigationEntries))
                {
                    return _navigationEntries
                        .SelectMany(pair => pair.Value)
                        .Where(entry => entry.NavigationType.IsRootNavigation)
                        .OrderByDescending(entry => entry.NavigationDate)
                        .FirstOrDefault();
                }

                return navigationEntries
                    .Where(entry => entry.NavigationProvider.Id == navigationEntry.NavigationProvider.Id)
                    .OrderByDescending(entry => entry.NavigationDate)
                    .FirstOrDefault();
            }
        }

        #endregion

        #region Methods

        private INavigationEntry GetNavigationEntry(IComponent<INavigationDispatcher>[] components, INavigationContext context)
        {
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as INavigationEntryProviderComponent)?.TryGetNavigationEntry(context);
                if (result != null)
                    return result;
            }

            return new NavigationEntry(context, _metadataContextProvider.DefaultIfNull().GetReadOnlyMetadataContext(this, context.Metadata));
        }

        private static bool CanAddNavigationEntry(IComponent<INavigationDispatcher>[] components, INavigationContext navigationContext)
        {
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionNavigationJournalComponent component && component.CanAddNavigationEntry(navigationContext))
                    return true;
            }

            return navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsBack || navigationContext.NavigationMode.IsNew;
        }

        private static bool CanRemoveNavigationEntry(IComponent<INavigationDispatcher>[] components, INavigationContext navigationContext)
        {
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionNavigationJournalComponent component && component.CanRemoveNavigationEntry(navigationContext))
                    return true;
            }

            return navigationContext.NavigationMode.IsClose;
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

        #endregion

        #region Nested types

        private sealed class NavigationEntry : INavigationEntry
        {
            #region Constructors

            public NavigationEntry(INavigationContext context, IReadOnlyMetadataContext metadata)
            {
                NavigationOperationId = context.NavigationOperationId;
                NavigationDate = DateTime.UtcNow;
                NavigationType = context.NavigationType;
                NavigationProvider = context.NavigationProvider;
                Metadata = metadata;
            }

            #endregion

            #region Properties

            public bool HasMetadata => Metadata.Count != 0;

            public IReadOnlyMetadataContext Metadata { get; }

            public string NavigationOperationId { get; }

            public DateTime NavigationDate { get; private set; }

            public NavigationType NavigationType { get; }

            public INavigationProvider NavigationProvider { get; }

            #endregion

            #region Methods

            public void UpdateNavigationDate()
            {
                NavigationDate = DateTime.UtcNow;
            }

            #endregion
        }

        #endregion
    }
}