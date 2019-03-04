using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Fields

        private readonly NavigationDispatcherListener _navigationListener;
        private IComponentCollection<IViewModelPresenterListener>? _listeners;
        private IComponentCollection<IChildViewModelPresenter>? _presenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(IViewModelPresenterCallbackManager callbackManager, INavigationDispatcher navigationDispatcher,
            IComponentCollection<IChildViewModelPresenter>? presenters = null, IComponentCollection<IViewModelPresenterListener>? listeners = null)
        {
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            CallbackManager = callbackManager;
            NavigationDispatcher = navigationDispatcher;
            _presenters = presenters;
            _listeners = listeners;
            _navigationListener = new NavigationDispatcherListener(this);
            navigationDispatcher.Listeners.Add(_navigationListener);
            CallbackManager.Initialize(this);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        public IViewModelPresenterCallbackManager CallbackManager { get; }

        public IComponentCollection<IChildViewModelPresenter> Presenters
        {
            get
            {
                if (_presenters == null)
                    _presenters = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IChildViewModelPresenter>(this, Default.MetadataContext);
                return _presenters;
            }
        }

        public IComponentCollection<IViewModelPresenterListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IViewModelPresenterListener>(this, Default.MetadataContext);
                return _listeners;
            }
        }

        public int NavigationDispatcherListenerPriority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_navigationListener.SuspendNavigation())
            {
                var result = ShowInternal(metadata);
                if (result == null)
                    throw ExceptionManager.PresenterCannotShowRequest(metadata.Dump());
                return OnShownInternal(metadata, result);
            }
        }

        public IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_navigationListener.SuspendNavigation())
            {
                var result = TryCloseInternal(metadata);
                return OnClosedInternal(metadata, result);
            }
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_navigationListener.SuspendNavigation())
            {
                var result = TryRestoreInternal(metadata);
                return OnRestoredInternal(metadata, result);
            }
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Count; i++)
            {
                var presenter = presenters[i];
                if (!CanShow(presenter, metadata))
                    continue;

                var operation = presenter.TryShow(this, metadata);
                if (operation != null)
                    return operation;
            }

            return null;
        }

        protected virtual IViewModelPresenterResult OnShownInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult result)
        {
            var r = result as IViewModelPresenterResult;
            if (r == null)
            {
                var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                if (viewModel == null)
                    throw ExceptionManager.PresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

                var showingCallback = CallbackManager.AddCallback(viewModel, NavigationCallbackType.Showing, result, metadata);
                var closeCallback = CallbackManager.AddCallback(viewModel, NavigationCallbackType.Close, result, metadata);

                r = new ViewModelPresenterResult(viewModel, showingCallback, closeCallback, result);
            }

            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnShown(this, metadata, r);

            return r;
        }

        protected virtual IReadOnlyList<IChildViewModelPresenterResult> TryCloseInternal(IReadOnlyMetadataContext metadata)
        {
            var results = new List<IChildViewModelPresenterResult>();
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Count; i++)
            {
                var presenter = presenters[i];
                if (!CanClose(presenter, results, metadata))
                    continue;

                var operations = presenter.TryClose(this, metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        protected virtual IReadOnlyList<IClosingViewModelPresenterResult> OnClosedInternal(IReadOnlyMetadataContext metadata, IReadOnlyList<IChildViewModelPresenterResult> results)
        {
            var r = new List<IClosingViewModelPresenterResult>();
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                if (result is IClosingViewModelPresenterResult closingViewModelPresenterResult)
                    r.Add(closingViewModelPresenterResult);
                else
                {
                    var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                    if (viewModel == null)
                        throw ExceptionManager.PresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

                    var callback = CallbackManager.AddCallback(viewModel, NavigationCallbackType.Closing, result, metadata);
                    r.Add(new ClosingViewModelPresenterResult((INavigationCallback<bool>)callback, result));
                }
            }


            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnClosed(this, metadata, r);

            return r;
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Count; i++)
            {
                if (presenters[i] is IRestorableChildViewModelPresenter presenter && CanRestore(presenter, metadata))
                {
                    var result = presenter.TryRestore(this, metadata);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        protected virtual IRestorationViewModelPresenterResult OnRestoredInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult? result)
        {
            var r = result == null
                ? RestorationViewModelPresenterResult.Unrestored
                : result as IRestorationViewModelPresenterResult ?? new RestorationViewModelPresenterResult(true, result);
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i]?.OnRestored(this, metadata, r);

            return r;
        }

        protected virtual bool CanShow(IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
            {
                var canShow = (listeners[i] as IConditionViewModelPresenterListener)?.CanShow(this, childPresenter, metadata) ?? true;
                if (!canShow)
                    return false;
            }

            return true;
        }

        protected virtual bool CanClose(IChildViewModelPresenter childPresenter, IReadOnlyList<IChildViewModelPresenterResult> currentResults, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
            {
                var canClose = (listeners[i] as IConditionViewModelPresenterListener)?.CanClose(this, childPresenter, currentResults, metadata) ?? true;
                if (!canClose)
                    return false;
            }

            return true;
        }

        protected virtual bool CanRestore(IRestorableChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = Listeners.GetItems();
            for (var i = 0; i < listeners.Count; i++)
            {
                var canRestore = (listeners[i] as IConditionViewModelPresenterListener)?.CanRestore(this, childPresenter, metadata) ?? true;
                if (!canRestore)
                    return false;
            }

            return true;
        }

        protected void UnsubscribeNavigationListener()
        {
            NavigationDispatcher.Listeners.Remove(_navigationListener);
        }

        #endregion

        #region Nested types

        private sealed class NavigationDispatcherListener : INavigationDispatcherListener, INavigationDispatcherJournalListener, IDisposable
        {
            #region Fields

            private readonly ViewModelPresenter _presenter;
            private readonly List<KeyValuePair<INavigationContext, object>> _suspendedEvents;
            private int _suspendCount;

            #endregion

            #region Constructors

            public NavigationDispatcherListener(ViewModelPresenter presenter)
            {
                _presenter = presenter;
                _suspendedEvents = new List<KeyValuePair<INavigationContext, object>>();
            }

            #endregion

            #region Implementation of interfaces

            public void Dispose()
            {
                KeyValuePair<INavigationContext, object>[] events;
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
                        OnNavigated(null, pair.Key);
                    else if (pair.Value is Exception e)
                        OnNavigationFailed(null, pair.Key, e);
                    else if (ReferenceEquals(pair.Key, pair.Value))
                        OnNavigationCanceled(null, pair.Key);
                    else
                        OnNavigatingCanceled(null, pair.Key);
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
                return _presenter.CallbackManager.GetCallbacks(navigationEntry, callbackType, metadata);
            }

            public Task<bool> OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                return Default.TrueTask;
            }

            public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(navigationContext, null));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigated(navigationContext);
            }

            public void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(navigationContext, exception));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigationFailed(navigationContext, exception);
            }

            public void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(navigationContext, navigationContext));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigationCanceled(navigationContext);
            }

            public void OnNavigatingCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(navigationContext, _suspendedEvents));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigatingCanceled(navigationContext);
            }

            public int GetPriority(object source)
            {
                return _presenter.NavigationDispatcherListenerPriority;
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