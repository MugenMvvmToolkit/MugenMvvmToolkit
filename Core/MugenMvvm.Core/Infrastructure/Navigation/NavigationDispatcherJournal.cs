using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationDispatcherJournal : AttachableComponentBase<INavigationDispatcher>, INavigationDispatcherJournal
    {
        #region Fields

        protected readonly Dictionary<NavigationType, List<WeakNavigationEntry>> NavigationEntries;
        private IComponentCollection<INavigationDispatcherJournalListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcherJournal(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
            NavigationEntries = new Dictionary<NavigationType, List<WeakNavigationEntry>>();
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public bool IsListenersInitialized => _listeners != null;

        public IComponentCollection<INavigationDispatcherJournalListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public void OnNavigated(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            OnNavigatedInternal(navigationContext);
        }

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return GetNavigationEntriesInternal(type, metadata);
        }

        public void UpdateNavigationEntries(Func<IReadOnlyDictionary<NavigationType, List<INavigationEntry>>, IReadOnlyMetadataContext, IReadOnlyDictionary<NavigationType, List<INavigationEntry>>> updateHandler,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(updateHandler, nameof(updateHandler));
            Should.NotBeNull(metadata, nameof(metadata));
            UpdateNavigationEntriesInternal(updateHandler, metadata);
        }

        public IReadOnlyList<INavigationCallback> GetCallbacks(INavigationEntry navigationEntry, NavigationCallbackType callbackType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetCallbacksInternal(navigationEntry, callbackType, metadata);
        }

        #endregion

        #region Methods

        protected virtual void OnNavigatedInternal(INavigationContext navigationContext)
        {
            lock (NavigationEntries)
            {
                if (navigationContext.ViewModelTo != null && CanAddNavigationEntry(navigationContext))
                {
                    if (!NavigationEntries.TryGetValue(navigationContext.NavigationTypeTo, out var list))
                    {
                        list = new List<WeakNavigationEntry>();
                        NavigationEntries[navigationContext.NavigationTypeTo] = list;
                    }

                    WeakNavigationEntry? viewModelRef = null;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var target = list[i].ViewModel;
                        if (target == null || ReferenceEquals(target, navigationContext.ViewModelTo))
                        {
                            if (target != null)
                                viewModelRef = list[i];
                            list.RemoveAt(i);
                            --i;
                        }
                    }

                    if (viewModelRef == null)
                        viewModelRef = new WeakNavigationEntry(navigationContext.ViewModelTo, navigationContext.NavigationProvider, navigationContext.NavigationTypeTo);
                    list.Add(viewModelRef);
                }

                if (navigationContext.ViewModelFrom != null && CanRemoveNavigationEntry(navigationContext))
                {
                    if (!NavigationEntries.TryGetValue(navigationContext.NavigationTypeFrom, out var list))
                    {
                        list = new List<WeakNavigationEntry>();
                        NavigationEntries[navigationContext.NavigationTypeFrom] = list;
                    }

                    for (var i = 0; i < list.Count; i++)
                    {
                        var target = list[i].ViewModel;
                        if (target == null || ReferenceEquals(target, navigationContext.ViewModelFrom))
                        {
                            list.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }

        protected virtual bool CanAddNavigationEntry(INavigationContext navigationContext)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i].CanAddNavigationEntry(this, navigationContext).GetValueOrDefault())
                    return true;
            }

            return navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsBack || navigationContext.NavigationMode.IsNew;
        }

        protected virtual bool CanRemoveNavigationEntry(INavigationContext navigationContext)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i].CanRemoveNavigationEntry(this, navigationContext).GetValueOrDefault())
                    return true;
            }


            return navigationContext.NavigationMode.IsClose;
        }

        protected virtual IReadOnlyList<INavigationEntry> GetNavigationEntriesInternal(NavigationType? type, IReadOnlyMetadataContext metadata)
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

        protected virtual IReadOnlyList<INavigationCallback> GetCallbacksInternal(INavigationEntry navigationEntry, NavigationCallbackType? callbackType,
            IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            if (listeners.Length == 0)
                return Default.EmptyArray<INavigationCallback>();

            List<INavigationCallback>? callbacks = null;
            for (var i = 0; i < listeners.Length; i++)
            {
                var list = listeners[i].GetCallbacks(this, navigationEntry, callbackType, metadata);
                if (list == null)
                    continue;
                if (callbacks == null)
                    callbacks = new List<INavigationCallback>();
                callbacks.AddRange(list);
            }

            if (callbacks == null)
                return Default.EmptyArray<INavigationCallback>();
            return callbacks;
        }

        protected virtual void UpdateNavigationEntriesInternal(Func<IReadOnlyDictionary<NavigationType, List<INavigationEntry>>, IReadOnlyMetadataContext,
            IReadOnlyDictionary<NavigationType, List<INavigationEntry>>> updateHandler, IReadOnlyMetadataContext metadata)
        {
            var oldEntries = new Dictionary<NavigationType, List<INavigationEntry>>();
            IReadOnlyDictionary<NavigationType, List<INavigationEntry>> newEntries;
            lock (NavigationEntries)
            {
                foreach (var navigationEntry in NavigationEntries)
                {
                    var entries = navigationEntry.Value.Select(entry => entry.ToNavigationEntry()).Where(entry => entry != null).ToList();
                    if (entries.Count != 0)
                        oldEntries[navigationEntry.Key] = entries;
                }
                NavigationEntries.Clear();
                newEntries = updateHandler(oldEntries, metadata);
                foreach (var entry in newEntries)
                {
                    var list = entry.Value;
                    if (list.Count == 0)
                        continue;
                    var entries = new List<WeakNavigationEntry>(list.Capacity);
                    for (int i = 0; i < list.Count; i++)
                        entries.Add(new WeakNavigationEntry(list[i]));
                    NavigationEntries[entry.Key] = entries;
                }
            }
            OnNavigationEntriesUpdated(oldEntries, newEntries, metadata);
        }

        protected virtual void OnNavigationEntriesUpdated(IReadOnlyDictionary<NavigationType, List<INavigationEntry>> oldEntries,
            IReadOnlyDictionary<NavigationType, List<INavigationEntry>> newEntries, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntriesUpdated(this, oldEntries, newEntries, metadata);
        }

        private void AddNavigationEntries(NavigationType type, ref List<INavigationEntry>? result)
        {
            if (!NavigationEntries.TryGetValue(type, out var list))
                return;
            if (result == null)
                result = new List<INavigationEntry>();
            var hasValue = false;
            for (var i = 0; i < list.Count; i++)
            {
                var target = list[i].ToNavigationEntry();
                if (target == null)
                {
                    list.RemoveAt(i);
                    --i;
                }
                else
                {
                    result.Add(target);
                    hasValue = true;
                }
            }

            if (!hasValue)
                NavigationEntries.Remove(type);
        }

        #endregion

        #region Nested types

        protected sealed class WeakNavigationEntry
        {
            #region Fields

            private readonly DateTime _date;
            private readonly IWeakReference _viewModelReference;

            #endregion

            #region Constructors

            public WeakNavigationEntry(IViewModelBase viewModel, INavigationProvider provider, NavigationType navigationType)
            {
                NavigationType = navigationType;
                NavigationProvider = provider;
                _viewModelReference = Service<IWeakReferenceProvider>.Instance.GetWeakReference(viewModel, Default.Metadata);
                _date = DateTime.UtcNow;
            }

            public WeakNavigationEntry(INavigationEntry navigationEntry)
            {
                NavigationType = navigationEntry.NavigationType;
                NavigationProvider = navigationEntry.NavigationProvider;
                _viewModelReference = Service<IWeakReferenceProvider>.Instance.GetWeakReference(navigationEntry.ViewModel, Default.Metadata);
                _date = navigationEntry.NavigationDate;
            }

            #endregion

            #region Properties

            public IViewModelBase? ViewModel => (IViewModelBase)_viewModelReference.Target;

            public INavigationProvider NavigationProvider { get; }

            public NavigationType NavigationType { get; }

            #endregion

            #region Methods

            public INavigationEntry? ToNavigationEntry()
            {
                var viewModel = ViewModel;
                var provider = NavigationProvider;
                if (viewModel == null)
                    return null;
                return new NavigationEntry(NavigationType, viewModel, _date, provider);
            }

            #endregion
        }

        protected sealed class NavigationEntry : INavigationEntry
        {
            #region Constructors

            public NavigationEntry(NavigationType type, IViewModelBase viewModel, DateTime date, INavigationProvider provider)
            {
                Should.NotBeNull(type, nameof(type));
                Should.NotBeNull(viewModel, nameof(viewModel));
                Should.NotBeNull(provider, nameof(provider));
                NavigationDate = date;
                NavigationType = type;
                NavigationProvider = provider;
                ViewModel = viewModel;
            }

            #endregion

            #region Properties

            public DateTime NavigationDate { get; }

            public NavigationType NavigationType { get; }

            public INavigationProvider NavigationProvider { get; }

            public IViewModelBase ViewModel { get; }

            #endregion
        }

        #endregion
    }
}