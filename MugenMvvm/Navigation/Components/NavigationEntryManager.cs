using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryManager : ComponentDecoratorBase<IPresenter, IPresenterComponent>, INavigationEntryProviderComponent, IPresenterComponent
    {
        private INavigationDispatcher? _navigationDispatcher;
        private readonly Dictionary<NavigationType, List<INavigationEntry>> _navigationEntries;
        private readonly NavigationEntryListener _navigationListener;

        [Preserve(Conditional = true)]
        public NavigationEntryManager(int priority = NavigationComponentPriority.EntryManager)
            : base(priority)
        {
            _navigationEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
            _navigationListener = new NavigationEntryListener(this);
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

        public ItemOrIReadOnlyList<INavigationEntry> TryGetNavigationEntries(INavigationDispatcher navigationDispatcher, IReadOnlyMetadataContext? metadata)
        {
            var result = new ItemOrListEditor<INavigationEntry>();
            lock (_navigationEntries)
            {
                foreach (var t in _navigationEntries)
                    result.AddRange(new ItemOrIEnumerable<INavigationEntry>(t.Value));
            }

            return result.ToItemOrList();
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryShow(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            using var token = _navigationListener.Suspend(request, metadata);
            var result = Components.TryShow(presenter, request, cancellationToken, metadata);
            foreach (var r in result)
                UpdateEntries(_navigationDispatcher.DefaultIfNull(), true, r.Target, r.NavigationProvider, r, true, r.GetMetadataOrDefault());
            return result;
        }

        public ItemOrIReadOnlyList<IPresenterResult> TryClose(IPresenter presenter, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => Components.TryClose(presenter, request, cancellationToken, metadata);

        private void UpdateEntries(INavigationDispatcher navigationDispatcher, bool isPending, INavigationContext navigationContext, bool isAdd)
            => UpdateEntries(navigationDispatcher, isPending, navigationContext.Target, navigationContext.NavigationProvider, navigationContext, isAdd,
                navigationContext.GetMetadataOrDefault());

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
                        addedEntry = new NavigationEntry(target, navigationProvider, navigationInfo.NavigationId, navigationInfo.NavigationType, metadata)
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
                    .GetComponents<INavigationEntryListener>(metadata)
                    .OnNavigationEntryAdded(navigationDispatcher, addedEntry, navigationInfo);
            }
            else if (updatedEntry != null && !isPending)
            {
                ((NavigationEntry)updatedEntry).IsPending = false;
                navigationDispatcher
                    .GetComponents<INavigationEntryListener>(metadata)
                    .OnNavigationEntryUpdated(navigationDispatcher, updatedEntry, navigationInfo);
            }
            else if (removedEntry != null)
            {
                navigationDispatcher
                    .GetComponents<INavigationEntryListener>(metadata)
                    .OnNavigationEntryRemoved(navigationDispatcher, removedEntry, navigationInfo);
            }
        }

        private sealed class NavigationEntryListener : SuspendableNavigationListenerBase, INavigationListener
        {
            private readonly NavigationEntryManager _entryManager;

            public NavigationEntryListener(NavigationEntryManager entryManager)
            {
                _entryManager = entryManager;
            }

            protected override void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
                => _entryManager.UpdateEntries(navigationDispatcher, !navigationContext.NavigationMode.IsNew, navigationContext, false);

            protected override void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
                => _entryManager.UpdateEntries(navigationDispatcher, !navigationContext.NavigationMode.IsNew, navigationContext, false);

            protected override void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
            }

            protected override void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
            }

            void INavigationListener.OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsNew)
                {
                    _entryManager.UpdateEntries(navigationDispatcher, true, navigationContext.Target, navigationContext.NavigationProvider, navigationContext,
                        !navigationContext.NavigationMode.IsClose, navigationContext.GetMetadataOrDefault());
                }
            }

            void INavigationListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                if (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsClose || navigationContext.NavigationMode.IsNew)
                {
                    _entryManager.UpdateEntries(navigationDispatcher, false, navigationContext.Target, navigationContext.NavigationProvider, navigationContext,
                        !navigationContext.NavigationMode.IsClose, navigationContext.GetMetadataOrDefault());
                }
            }
        }

        public override void OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is INavigationDispatcher navigationDispatcher)
            {
                if (Interlocked.CompareExchange(ref _navigationDispatcher, navigationDispatcher, null) != null)
                    ExceptionManager.ThrowObjectInitialized(this);
                navigationDispatcher.AddComponent(_navigationListener);
            }

            base.OnAttached(owner, metadata);
        }

        public override void OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is INavigationDispatcher navigationDispatcher)
            {
                if (Interlocked.CompareExchange(ref _navigationDispatcher, null, navigationDispatcher) == navigationDispatcher)
                    navigationDispatcher.RemoveComponent(_navigationListener);
            }

            base.OnDetached(owner, metadata);
        }
    }
}