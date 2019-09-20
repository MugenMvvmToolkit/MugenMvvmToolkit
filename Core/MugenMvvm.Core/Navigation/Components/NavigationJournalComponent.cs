using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public class NavigationJournalComponent : AttachableComponentBase<INavigationDispatcher>, INavigationJournalComponent, INavigationDispatcherNavigatedListener, IHasPriority
    {
        #region Fields

        protected readonly Dictionary<NavigationType, List<INavigationEntry>> NavigationEntries;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationJournalComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            NavigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
        }

        #endregion

        #region Properties

        protected IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.ServiceIfNull();

        public int Priority => int.MaxValue;

        #endregion

        #region Implementation of interfaces

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            OnNavigatedInternal(navigationContext);
        }

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext? metadata = null)
        {
            return GetNavigationEntriesInternal(type, metadata);
        }

        public INavigationEntry? GetNavigationEntryById(string navigationOperationId, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationOperationId, nameof(navigationOperationId));
            return GetNavigationEntryByIdInternal(navigationOperationId, metadata);
        }

        public INavigationEntry? GetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            return GetPreviousNavigationEntryInternal(navigationEntry, metadata);
        }

        #endregion

        #region Methods

        protected virtual void OnNavigatedInternal(INavigationContext navigationContext)
        {
            INavigationEntry? addedEntry = null;
            INavigationEntry? updatedEntry = null;
            INavigationEntry? removedEntry = null;
            lock (NavigationEntries)
            {
                if (CanAddNavigationEntry(navigationContext))
                {
                    if (!NavigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        list = new List<INavigationEntry>();
                        NavigationEntries[navigationContext.NavigationType] = list;
                    }

                    updatedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                    if (updatedEntry == null)
                    {
                        addedEntry = GetNavigationEntry(navigationContext);
                        list.Add(addedEntry);
                    }
                    else if (updatedEntry is NavigationEntry entry)
                        entry.UpdateNavigationDate();
                }
                else if (CanRemoveNavigationEntry(navigationContext))
                {
                    if (NavigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                    {
                        removedEntry = FindEntry(list, navigationContext.NavigationOperationId);
                        if (removedEntry != null)
                            list.Remove(removedEntry);
                    }
                }
            }

            if (addedEntry != null)
                OnNavigationEntryAdded(addedEntry);
            else if (updatedEntry != null)
                OnNavigationEntryUpdated(updatedEntry);
            else if (removedEntry != null)
                OnNavigationEntryRemoved(removedEntry);
        }

        protected virtual IReadOnlyList<INavigationEntry> GetNavigationEntriesInternal(NavigationType? type, IReadOnlyMetadataContext? metadata)
        {
            lock (NavigationEntries)
            {
                List<INavigationEntry>? result = null;
                if (type == null)
                {
                    foreach (var t in NavigationEntries)
                        AddNavigationEntries(t.Key, ref result);
                }
                else
                    AddNavigationEntries(type, ref result);

                if (result == null)
                    return Default.EmptyArray<INavigationEntry>();
                return result;
            }
        }

        protected INavigationEntry? GetNavigationEntryByIdInternal(string navigationOperationId, IReadOnlyMetadataContext? metadata)
        {
            IEnumerable<INavigationEntry>? entries = null;
            lock (NavigationEntries)
            {
                var components = Owner.GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    var navigationEntry = (components[i] as INavigationEntryFinderComponent)
                        ?.TryGetGetNavigationEntryById(entries ??= NavigationEntries.SelectMany(pair => pair.Value), navigationOperationId, metadata);
                    if (navigationEntry != null)
                        return navigationEntry;
                }

                foreach (var navigationEntry in NavigationEntries)
                {
                    var findEntry = FindEntry(navigationEntry.Value, navigationOperationId);
                    if (findEntry != null)
                        return findEntry;
                }

                return null;
            }
        }

        protected virtual INavigationEntry? GetPreviousNavigationEntryInternal(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata)
        {
            IEnumerable<INavigationEntry>? entries = null;
            lock (NavigationEntries)
            {
                var components = Owner.GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    var result = (components[i] as INavigationEntryFinderComponent)
                        ?.TryGetPreviousNavigationEntry(entries ??= NavigationEntries.SelectMany(pair => pair.Value), navigationEntry, metadata);
                    if (result != null)
                        return result;
                }

                if (navigationEntry.NavigationType.IsUndefined)
                    return null;

                if (navigationEntry.NavigationType.IsNestedNavigation)
                {
                    if (!NavigationEntries.TryGetValue(navigationEntry.NavigationType, out var list))
                        return null;
                    return list
                        .Where(entry => entry.NavigationProvider.Id == navigationEntry.NavigationProvider.Id)
                        .OrderByDescending(entry => entry.NavigationDate)
                        .FirstOrDefault();
                }

                if (navigationEntry.NavigationType.IsSystemNavigation || !NavigationEntries.TryGetValue(navigationEntry.NavigationType, out var navigationEntries))
                {
                    return NavigationEntries
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

        protected virtual INavigationEntry GetNavigationEntry(INavigationContext context)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as INavigationEntryProviderComponent)?.TryGetNavigationEntry(context);
                if (result != null)
                    return result;
            }

            return new NavigationEntry(context, MetadataContextProvider.GetReadOnlyMetadataContext(this, context.Metadata));
        }

        protected virtual void OnNavigationEntryAdded(INavigationEntry navigationEntry)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as INavigationJournalListener)?.OnNavigationEntryAdded(this, navigationEntry);
        }

        protected virtual void OnNavigationEntryUpdated(INavigationEntry navigationEntry)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as INavigationJournalListener)?.OnNavigationEntryUpdated(this, navigationEntry);
        }

        protected virtual void OnNavigationEntryRemoved(INavigationEntry navigationEntry)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as INavigationJournalListener)?.OnNavigationEntryRemoved(this, navigationEntry);
        }

        protected virtual bool CanAddNavigationEntry(INavigationContext navigationContext)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionNavigationJournalComponent component && component.CanAddNavigationEntry(navigationContext))
                    return true;
            }

            return navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsBack || navigationContext.NavigationMode.IsNew;
        }

        protected virtual bool CanRemoveNavigationEntry(INavigationContext navigationContext)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionNavigationJournalComponent component && component.CanRemoveNavigationEntry(navigationContext))
                    return true;
            }

            return navigationContext.NavigationMode.IsClose;
        }

        protected static INavigationEntry? FindEntry(List<INavigationEntry> entries, string id)
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
            if (NavigationEntries.TryGetValue(type, out var list))
            {
                if (result == null)
                    result = list.ToList();
                else
                    result.AddRange(list);
            }
        }

        #endregion

        #region Nested types

        protected sealed class NavigationEntry : INavigationEntry
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

            public bool HasMetadata => true;

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