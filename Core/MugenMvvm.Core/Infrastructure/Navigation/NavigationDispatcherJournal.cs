using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationDispatcherJournal : INavigationDispatcherJournal //todo update can add/remove entry, add navigationfromtype
    {
        #region Fields

        protected readonly Dictionary<NavigationType, List<WeakNavigationEntry>> NavigationEntries;
        private IComponentCollection<INavigationDispatcherJournalListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcherJournal(IComponentCollection<INavigationDispatcherJournalListener>? listeners = null)
        {
            _listeners = listeners;
            NavigationEntries = new Dictionary<NavigationType, List<WeakNavigationEntry>>();
        }

        #endregion

        #region Properties

        public IComponentCollection<INavigationDispatcherJournalListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<INavigationDispatcherJournalListener>(this, Default.MetadataContext);
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

        #endregion

        #region Methods

        protected virtual void OnNavigatedInternal(INavigationContext navigationContext)
        {
            var viewModelFrom = navigationContext.Metadata.Get(NavigationInternalMetadata.ViewModelFromNavigationType, navigationContext.NavigationType) ==
                                navigationContext.NavigationType
                ? navigationContext.ViewModelFrom
                : null;
            var viewModelTo = navigationContext.Metadata.Get(NavigationInternalMetadata.ViewModelToNavigationType, navigationContext.NavigationType) ==
                              navigationContext.NavigationType
                ? navigationContext.ViewModelTo
                : null;

            lock (NavigationEntries)
            {
                if (!NavigationEntries.TryGetValue(navigationContext.NavigationType, out var list))
                {
                    list = new List<WeakNavigationEntry>();
                    NavigationEntries[navigationContext.NavigationType] = list;
                }

                if (viewModelTo != null && (navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsBack || navigationContext.NavigationMode.IsNew))
                {
                    WeakNavigationEntry? viewModelRef = null;
                    for (var i = 0; i < list.Count; i++)
                    {
                        var target = list[i].ViewModel;
                        if (target == null || ReferenceEquals(target, viewModelTo))
                        {
                            if (target != null)
                                viewModelRef = list[i];
                            list.RemoveAt(i);
                            --i;
                        }
                    }

                    if (viewModelRef == null)
                        viewModelRef = new WeakNavigationEntry(this, viewModelTo, navigationContext.NavigationProvider, navigationContext.NavigationType);
                    list.Add(viewModelRef);
                }

                if (viewModelFrom != null && navigationContext.NavigationMode.IsClose)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var target = list[i].ViewModel;
                        if (target == null || ReferenceEquals(target, viewModelFrom))
                        {
                            list.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }

        protected bool CanAddNavigationEntry(INavigationContext navigationContext)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].CanAddNavigationEntry(this, navigationContext).GetValueOrDefault())
                    return true;
            }


            return false;
        }

        protected bool CanRemoveNavigationEntry(INavigationContext navigationContext)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].CanRemoveNavigationEntry(this, navigationContext).GetValueOrDefault())
                    return true;
            }


            return false;
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
            var listeners = Listeners.GetItems();
            if (listeners.Count == 0)
                return Default.EmptyArray<INavigationCallback>();

            List<INavigationCallback>? callbacks = null;
            for (var i = 0; i < listeners.Count; i++)
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

            private readonly NavigationDispatcherJournal _navigationDispatcherJournal;
            private readonly WeakReference _viewModelReference;

            #endregion

            #region Constructors

            public WeakNavigationEntry(NavigationDispatcherJournal navigationDispatcherJournal, IViewModelBase viewModel, INavigationProvider provider,
                NavigationType navigationType)
            {
                _navigationDispatcherJournal = navigationDispatcherJournal;
                NavigationType = navigationType;
                NavigationProvider = provider;
                _viewModelReference = MugenExtensions.GetWeakReference(viewModel);
                _date = DateTime.UtcNow;
            }

            #endregion

            #region Properties

            public IViewModelBase? ViewModel => (IViewModelBase) _viewModelReference.Target;

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
                return new NavigationEntry(_navigationDispatcherJournal, NavigationType, viewModel, _date, provider);
            }

            #endregion
        }

        protected sealed class NavigationEntry : INavigationEntry
        {
            #region Fields

            private readonly NavigationDispatcherJournal _navigationDispatcherJournal;

            #endregion

            #region Constructors

            public NavigationEntry(NavigationDispatcherJournal navigationDispatcherJournal, NavigationType type, IViewModelBase viewModel, DateTime date,
                INavigationProvider provider)
            {
                Should.NotBeNull(type, nameof(type));
                Should.NotBeNull(viewModel, nameof(viewModel));
                Should.NotBeNull(provider, nameof(provider));
                _navigationDispatcherJournal = navigationDispatcherJournal;
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

            #region Implementation of interfaces

            public IReadOnlyList<INavigationCallback> GetCallbacks(NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata)
            {
                Should.NotBeNull(callbackType, nameof(callbackType));
                Should.NotBeNull(metadata, nameof(metadata));
                return _navigationDispatcherJournal.GetCallbacksInternal(this, callbackType, metadata);
            }

            #endregion
        }

        #endregion
    }
}