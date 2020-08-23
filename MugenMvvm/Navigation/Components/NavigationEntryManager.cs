﻿using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryManager : ComponentDecoratorBase<IPresenter, IPresenterComponent>, INavigationEntryProviderComponent,
        INavigationDispatcherNavigatingListener, INavigationDispatcherNavigatedListener, INavigationDispatcherErrorListener, IPresenterComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly Dictionary<NavigationType, List<INavigationEntry>> _navigationEntries;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationEntryManager(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
            _navigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Max;

        #endregion

        #region Implementation of interfaces

        public void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
            => UpdateEntries(navigationDispatcher, true, navigationContext, false);

        public void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
            => UpdateEntries(navigationDispatcher, true, navigationContext, false);

        public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsNew)
                UpdateEntries(navigationDispatcher, true, navigationContext.Target, navigationContext.NavigationProvider, navigationContext, !navigationContext.NavigationMode.IsClose,
                    navigationContext.GetMetadataOrDefault());
        }

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsClose || navigationContext.NavigationMode.IsNew)
                UpdateEntries(navigationDispatcher, false, navigationContext.Target, navigationContext.NavigationProvider, navigationContext, !navigationContext.NavigationMode.IsClose,
                    navigationContext.GetMetadataOrDefault());
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

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var result = Components.TryShow(presenter, request, cancellationToken, metadata);
            foreach (var r in result.Iterator())
                UpdateEntries(_navigationDispatcher.DefaultIfNull(), true, r.Target, r.NavigationProvider, r, true, r.GetMetadataOrDefault());
            return result;
        }

        public ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => Components.TryClose(presenter, request, cancellationToken, metadata);

        #endregion

        #region Methods

        private void UpdateEntries(INavigationDispatcher navigationDispatcher, bool isPending, INavigationContext navigationContext, bool isAdd)
            => UpdateEntries(navigationDispatcher, isPending, navigationContext.Target, navigationContext.NavigationProvider, navigationContext, isAdd, navigationContext.GetMetadataOrDefault());

        private void UpdateEntries(INavigationDispatcher navigationDispatcher, bool isPending, object? target, INavigationProvider navigationProvider,
            IHasNavigationInfo navigationInfo, bool isAdd, IReadOnlyMetadataContext? metadata)
        {
            INavigationEntry? addedEntry = null;
            INavigationEntry? updatedEntry = null;
            INavigationEntry? removedEntry = null;
            lock (_navigationEntries)
            {
                if (isAdd)
                {
                    if (!_navigationEntries.TryGetValue(navigationInfo.NavigationType, out var list))
                    {
                        list = new List<INavigationEntry>();
                        _navigationEntries[navigationInfo.NavigationType] = list;
                    }

                    updatedEntry = FindEntry(list, navigationInfo.NavigationId);
                    if (updatedEntry == null)
                    {
                        addedEntry = new NavigationEntry(target, navigationProvider, navigationInfo.NavigationId, navigationInfo.NavigationType, metadata.ToNonReadonly())
                        {
                            IsPending = isPending
                        };
                        list.Add(addedEntry);
                    }
                }
                else
                {
                    if (_navigationEntries.TryGetValue(navigationInfo.NavigationType, out var list))
                    {
                        removedEntry = FindEntry(list, navigationInfo.NavigationId);
                        if (removedEntry != null && (!isPending || removedEntry.IsPending))
                            list.Remove(removedEntry);
                    }
                }
            }

            if (addedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(metadata)
                    .OnNavigationEntryAdded(navigationDispatcher, addedEntry, navigationInfo);
            }
            else if (updatedEntry != null)
            {
                if (!isPending)
                    ((NavigationEntry)updatedEntry).IsPending = false;
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(metadata)
                    .OnNavigationEntryUpdated(navigationDispatcher, updatedEntry, navigationInfo);
            }
            else if (removedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationDispatcherEntryListener>(metadata)
                    .OnNavigationEntryRemoved(navigationDispatcher, removedEntry, navigationInfo);
            }
        }

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