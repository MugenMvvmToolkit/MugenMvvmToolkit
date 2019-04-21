using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenterCallbackManager : AttachableComponentBase<IViewModelPresenter>, IViewModelPresenterCallbackManager
    {
        #region Fields

        private readonly NavigationDispatcherListener _dispatcherListener;
        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private IComponentCollection<IViewModelPresenterCallbackManagerListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenterCallbackManager(INavigationDispatcher navigationDispatcher, IComponentCollectionProvider? componentCollectionProvider = null)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            _componentCollectionProvider = componentCollectionProvider;
            NavigationDispatcher = navigationDispatcher;
            _dispatcherListener = new NavigationDispatcherListener(this);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        public IComponentCollection<IViewModelPresenterCallbackManagerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this, _componentCollectionProvider);
                return _listeners;
            }
        }

        public bool IsSerializable { get; set; }

        public int NavigationDispatcherListenerPriority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IDisposable BeginPresenterOperation(IReadOnlyMetadataContext metadata)
        {
            return _dispatcherListener.SuspendNavigation();
        }

        public INavigationCallback<T> AddCallback<T>(IViewModelBase viewModel, NavigationCallbackType callbackType,
            IChildViewModelPresenterResult presenterResult, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(presenterResult, nameof(presenterResult));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(presenterResult, nameof(presenterResult));
            Should.NotBeNull(metadata, nameof(metadata));
            var callback = AddCallbackInternal<T>(viewModel, callbackType, presenterResult);
            OnCallbackAdded(viewModel, callback, presenterResult);
            return callback;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IViewModelPresenter owner, IReadOnlyMetadataContext metadata)
        {
            NavigationDispatcher.AddListener(_dispatcherListener);
            NavigationDispatcher.NavigationJournal.AddListener(_dispatcherListener);
        }

        protected override void OnDetachedInternal(IViewModelPresenter owner, IReadOnlyMetadataContext metadata)
        {
            NavigationDispatcher.RemoveListener(_dispatcherListener);
            NavigationDispatcher.NavigationJournal.RemoveListener(_dispatcherListener);
        }

        protected virtual INavigationCallback<T> AddCallbackInternal<T>(IViewModelBase viewModel, NavigationCallbackType callbackType,
            IChildViewModelPresenterResult presenterResult)
        {
            var serializable = IsSerializable && callbackType == NavigationCallbackType.Close && presenterResult.Metadata.Get(NavigationInternalMetadata.IsRestorableCallback);
            var callback = new NavigationCallback<T>(callbackType, presenterResult.NavigationType, serializable, presenterResult.NavigationProvider.Id);

            var key = GetKeyByCallback(callbackType);
            if (key == null)
                ExceptionManager.ThrowEnumOutOfRange(nameof(callbackType), callbackType);

            var callbacks = viewModel.Metadata.GetOrAdd(key!, (object?)null, (object?)null, (context, o, arg3) => new List<INavigationCallbackInternal?>())!;
            lock (callback)
            {
                callbacks.Add(callback);
            }

            return callback;
        }

        protected virtual void OnNavigatedInternal(INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.ShowingCallbacks!, Default.TrueObject, null, false);
                InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.ClosingCallbacks!, Default.TrueObject, null, false);
                InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.CloseCallbacks, Default.TrueObject, null, false);
            }
            else
                InvokeCallbacks(navigationContext, false, NavigationInternalMetadata.ShowingCallbacks!, Default.TrueObject, null, false);
        }

        protected virtual void OnNavigationFailedInternal(INavigationContext navigationContext, Exception exception)
        {
            OnNavigationFailed(navigationContext, exception, false);
        }

        protected virtual void OnNavigationCanceledInternal(INavigationContext navigationContext)
        {
            OnNavigationFailed(navigationContext, null, true);
        }

        protected virtual void OnNavigatingCanceledInternal(INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
                InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.ClosingCallbacks!, Default.FalseObject, null, false);
        }

        protected virtual IReadOnlyList<INavigationCallback> GetCallbacksInternal(INavigationEntry navigationEntry, NavigationCallbackType? callbackType,
            IReadOnlyMetadataContext metadata)
        {
            List<INavigationCallback>? callbacks = null;
            if (callbackType == null)
            {
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.ShowingCallbacks!, ref callbacks);
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.ClosingCallbacks!, ref callbacks);
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.CloseCallbacks, ref callbacks);
            }
            else
            {
                var key = GetKeyByCallback(callbackType);
                if (key != null)
                    AddEntryCallbacks(navigationEntry, key, ref callbacks);
            }

            if (callbacks == null)
                return Default.EmptyArray<INavigationCallback>();
            return callbacks;
        }

        protected virtual IMetadataContextKey<IList<INavigationCallbackInternal?>?>? GetKeyByCallback(NavigationCallbackType callbackType)
        {
            if (callbackType == NavigationCallbackType.Showing)
                return NavigationInternalMetadata.ShowingCallbacks!;
            if (callbackType == NavigationCallbackType.Closing)
                return NavigationInternalMetadata.ClosingCallbacks!;
            if (callbackType == NavigationCallbackType.Close)
                return NavigationInternalMetadata.CloseCallbacks;
            return null;
        }

        protected virtual void OnCallbackAdded(IViewModelBase viewModel, INavigationCallback callback, IChildViewModelPresenterResult presenterResult)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCallbackAdded(this, viewModel, callback, presenterResult);
        }

        protected virtual void OnCallbackExecuted(IViewModelBase viewModel, INavigationCallback callback, INavigationContext navigationContext)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCallbackExecuted(this, viewModel, callback, navigationContext);
        }

        protected void InvokeCallbacks(INavigationContext navigationContext, bool isFromNavigation, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, object? result,
            Exception? exception, bool canceled)
        {
            IViewModelBase? viewModel;
            NavigationType navigationType;
            if (isFromNavigation)
            {
                viewModel = navigationContext.ViewModelFrom;
                navigationType = navigationContext.NavigationTypeFrom;
            }
            else
            {
                viewModel = navigationContext.ViewModelTo;
                navigationType = navigationContext.NavigationTypeTo;
            }

            var callbacks = viewModel?.Metadata.Get(key);
            if (callbacks == null)
                return;

            List<INavigationCallbackInternal>? toInvoke = null;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationProviderId == navigationContext.NavigationProvider.Id && callback.NavigationType == navigationType)
                    {
                        if (callback != null)
                        {
                            if (toInvoke == null)
                                toInvoke = new List<INavigationCallbackInternal>();
                            toInvoke.Add(callback);
                        }

                        callbacks.RemoveAt(i);
                        --i;
                    }
                }
            }

            if (toInvoke == null)
                return;
            for (var i = 0; i < toInvoke.Count; i++)
            {
                var callback = toInvoke[i];
                if (exception != null)
                    callback.SetException(exception, navigationContext);
                else if (canceled)
                    callback.SetCanceled(navigationContext);
                else
                    callback.SetResult(result, navigationContext);
                OnCallbackExecuted(viewModel!, callback, navigationContext);
            }
        }

        protected IViewModelPresenterCallbackManagerListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        private void OnNavigationFailed(INavigationContext navigationContext, Exception? e, bool canceled)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.ClosingCallbacks!, Default.FalseObject, e, canceled);
                if (!canceled)
                {
                    InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.ShowingCallbacks!, Default.FalseObject, e, false);
                    InvokeCallbacks(navigationContext, true, NavigationInternalMetadata.CloseCallbacks, Default.FalseObject, e, false);
                }
            }
            else
            {
                InvokeCallbacks(navigationContext, false, NavigationInternalMetadata.ShowingCallbacks!, Default.FalseObject, e, canceled);
                if (navigationContext.NavigationMode.IsNew || !canceled)
                {
                    InvokeCallbacks(navigationContext, false, NavigationInternalMetadata.ClosingCallbacks!, Default.FalseObject, e, canceled);
                    InvokeCallbacks(navigationContext, false, NavigationInternalMetadata.CloseCallbacks, Default.FalseObject, e, canceled);
                }
            }
        }

        private static void AddEntryCallbacks(INavigationEntry navigationEntry, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key,
            ref List<INavigationCallback>? callbacks)
        {
            var list = navigationEntry.ViewModel.Metadata.Get(key);
            if (list == null)
                return;
            if (callbacks == null)
                callbacks = new List<INavigationCallback>();
            lock (list)
            {
                callbacks.AddRange(list.Where(c => c != null)!);
            }
        }

        #endregion

        #region Nested types

        private sealed class NavigationDispatcherListener : INavigationDispatcherListener, INavigationDispatcherJournalListener, IDisposable
        {
            #region Fields

            private readonly ViewModelPresenterCallbackManager _callbackManager;
            private readonly List<KeyValuePair<INavigationContext, object?>> _suspendedEvents;
            private int _suspendCount;

            #endregion

            #region Constructors

            public NavigationDispatcherListener(ViewModelPresenterCallbackManager callbackManager)
            {
                _callbackManager = callbackManager;
                _suspendedEvents = new List<KeyValuePair<INavigationContext, object?>>();
            }

            #endregion

            #region Implementation of interfaces

            public void Dispose()
            {
                KeyValuePair<INavigationContext, object?>[] events;
                lock (_suspendedEvents)
                {
                    if (--_suspendCount != 0)
                        return;
                    events = _suspendedEvents.ToArray();
                    _suspendedEvents.Clear();
                }

                for (var i = 0; i < events.Length; i++)
                {
                    var pair = events[i];
                    if (pair.Value == null)
                        OnNavigated(null!, pair.Key);
                    else if (pair.Value is Exception e)
                        OnNavigationFailed(null!, pair.Key, e);
                    else if (ReferenceEquals(pair.Key, pair.Value))
                        OnNavigationCanceled(null!, pair.Key);
                    else
                        OnNavigatingCanceled(null!, pair.Key);
                }
            }

            public bool? CanAddNavigationEntry(INavigationDispatcherJournal navigationDispatcherJournal, INavigationContext navigationContext)
            {
                return null;
            }

            public bool? CanRemoveNavigationEntry(INavigationDispatcherJournal navigationDispatcherJournal, INavigationContext navigationContext)
            {
                return null;
            }

            public IReadOnlyList<INavigationCallback> GetCallbacks(INavigationDispatcherJournal navigationDispatcherJournal, INavigationEntry navigationEntry,
                NavigationCallbackType? callbackType,
                IReadOnlyMetadataContext metadata)
            {
                return _callbackManager.GetCallbacksInternal(navigationEntry, callbackType, metadata);
            }

            public Task<bool>? OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                return null;
            }

            public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object?>(navigationContext, null));
                        return;
                    }
                }

                _callbackManager.OnNavigatedInternal(navigationContext);
            }

            public void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object?>(navigationContext, exception));
                        return;
                    }
                }

                _callbackManager.OnNavigationFailedInternal(navigationContext, exception);
            }

            public void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object?>(navigationContext, navigationContext));
                        return;
                    }
                }

                _callbackManager.OnNavigationCanceledInternal(navigationContext);
            }

            public void OnNavigatingCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object?>(navigationContext, _suspendedEvents));
                        return;
                    }
                }

                _callbackManager.OnNavigatingCanceledInternal(navigationContext);
            }

            public int GetPriority(object source)
            {
                return _callbackManager.NavigationDispatcherListenerPriority;
            }

            #endregion

            #region Methods

            public NavigationDispatcherListener SuspendNavigation()
            {
                lock (_suspendedEvents)
                {
                    ++_suspendCount;
                }

                return this;
            }

            #endregion
        }

        #endregion
    }
}