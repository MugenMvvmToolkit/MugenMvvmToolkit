using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenter : HasListenersBase<IViewModelPresenterListener>, IViewModelPresenter//todo tracer remove to listeners, return empty metadata
    {
        #region Fields

        private readonly PresentersCollection _presenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(INavigationDispatcher navigationDispatcher, IViewModelPresenterCallbackManager callbackManager, ITracer tracer)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            Should.NotBeNull(tracer, nameof(tracer));
            NavigationDispatcher = navigationDispatcher;
            CallbackManager = callbackManager;
            Tracer = tracer;
            _presenters = new PresentersCollection(this);
            navigationDispatcher.AddListener(_presenters);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected ITracer Tracer { get; }

        public IViewModelPresenterCallbackManager CallbackManager { get; }

        public ICollection<IChildViewModelPresenter> Presenters => _presenters;

        #endregion

        #region Implementation of interfaces

        public IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_presenters.SuspendNavigation())
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
            using (_presenters.SuspendNavigation())
            {
                var result = TryCloseInternal(metadata);
                return OnClosedInternal(metadata, result);
            }
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_presenters.SuspendNavigation())
            {
                var result = TryRestoreInternal(metadata);
                return OnRestoredInternal(metadata, result);
            }
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                var presenter = presenters[i];
                if (!CanShow(presenter, metadata))
                    continue;

                var operation = presenter.TryShow(this, metadata);
                if (operation != null)
                {
                    Trace("show", metadata, presenters[i], operation);
                    return operation;
                }
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

                var showingCallback = CallbackManager.AddCallback(this, viewModel, NavigationCallbackType.Showing, result, metadata);
                var closeCallback = CallbackManager.AddCallback(this, viewModel, NavigationCallbackType.Close, result, metadata);

                r = new ViewModelPresenterResult(viewModel, showingCallback, closeCallback, result);
            }

            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnShown(this, metadata, r);
            }

            return r;
        }

        protected virtual IReadOnlyList<IChildViewModelPresenterResult> TryCloseInternal(IReadOnlyMetadataContext metadata)
        {
            var results = new List<IChildViewModelPresenterResult>();
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
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
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];

                if (result is IClosingViewModelPresenterResult closingViewModelPresenterResult)
                    r.Add(closingViewModelPresenterResult);
                else
                {
                    var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                    if (viewModel == null)
                        throw ExceptionManager.PresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

                    var callback = CallbackManager.AddCallback(this, viewModel, NavigationCallbackType.Closing, result, metadata);
                    r.Add(new ClosingViewModelPresenterResult((INavigationCallback<bool>)callback, result));
                }
            }


            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnClosed(this, metadata, r);
            }

            return r;
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] is IRestorableChildViewModelPresenter presenter && CanRestore(presenter, metadata))
                {
                    var result = presenter.TryRestore(this, metadata);
                    if (result != null)
                    {
                        Trace("restore", metadata, presenter, result);
                        return result;
                    }
                }
            }

            return null;
        }

        protected virtual IRestorationViewModelPresenterResult OnRestoredInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult? result)
        {
            var r = result == null ? RestorationViewModelPresenterResult.Unrestored
                : (result as IRestorationViewModelPresenterResult ?? new RestorationViewModelPresenterResult(true, result));
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnRestored(this, metadata, r);
            }

            return r;
        }

        protected virtual void OnChildPresenterAdded(IChildViewModelPresenter presenter)
        {
        }

        protected virtual void OnChildPresenterRemoved(IChildViewModelPresenter presenter)
        {
        }

        protected virtual bool CanShow(IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Length; i++)
                {
                    var canShow = (listeners[i] as IConditionViewModelPresenterListener)?.CanShow(this, childPresenter, metadata) ?? true;
                    if (!canShow)
                        return false;
                }
            }

            return true;
        }

        protected virtual bool CanClose(IChildViewModelPresenter childPresenter, IReadOnlyList<IChildViewModelPresenterResult> currentResults, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Length; i++)
                {
                    var canClose = (listeners[i] as IConditionViewModelPresenterListener)?.CanClose(this, childPresenter, currentResults, metadata) ?? true;
                    if (!canClose)
                        return false;
                }
            }

            return true;
        }

        protected virtual bool CanRestore(IRestorableChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Length; i++)
                {
                    var canRestore = (listeners[i] as IConditionViewModelPresenterListener)?.CanRestore(this, childPresenter, metadata) ?? true;
                    if (!canRestore)
                        return false;
                }
            }

            return true;
        }

        private void Trace(string requestName, IReadOnlyMetadataContext metadata, IChildViewModelPresenter presenter,
            IHasMetadata<IReadOnlyMetadataContext> hasMetadata) //todo remove all traces
        {
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Info(MessageConstants.TraceViewModelPresenterFormat3, requestName, metadata.Dump() + hasMetadata.Metadata.Dump(), presenter.GetType().FullName);
        }

        #endregion

        #region Nested types

        private sealed class PresentersCollection : ICollection<IChildViewModelPresenter>, IComparer<IChildViewModelPresenter>, INavigationDispatcherCallbackProvider, IDisposable
        {
            #region Fields

            private readonly OrderedListInternal<IChildViewModelPresenter> _list;
            private readonly ViewModelPresenter _presenter;
            private readonly List<KeyValuePair<INavigationContext, object>> _suspendedEvents;
            private int _suspendCount;

            #endregion

            #region Constructors

            public PresentersCollection(ViewModelPresenter presenter)
            {
                _presenter = presenter;
                _list = new OrderedListInternal<IChildViewModelPresenter>(comparer: this);
                _suspendedEvents = new List<KeyValuePair<INavigationContext, object>>();
            }

            #endregion

            #region Properties

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            #endregion

            #region Implementation of interfaces

            public Task<bool> OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext context)
            {
                return Default.TrueTask;
            }

            public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext context)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, null));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigated(_presenter, context);
            }

            public void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext context, Exception exception)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, exception));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigationFailed(_presenter, context, exception);
            }

            public void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext context)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, context));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigationCanceled(_presenter, context);
            }

            public void OnNavigatingCanceled(INavigationDispatcher navigationDispatcher, INavigationContext context)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, _suspendedEvents));
                        return;
                    }
                }

                _presenter.CallbackManager.OnNavigatingCanceled(_presenter, context);
            }

            public IReadOnlyList<INavigationCallback> GetCallbacks(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry,
                NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata)
            {
                return _presenter.CallbackManager.GetCallbacks(_presenter, navigationEntry, callbackType, metadata);
            }

            public IEnumerator<IChildViewModelPresenter> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IChildViewModelPresenter item)
            {
                Should.NotBeNull(item, nameof(item));
                _list.Add(item);
                _presenter.OnChildPresenterAdded(item);
            }

            public void Clear()
            {
                var values = _list.ToArray();
                _list.Clear();
                for (var index = 0; index < values.Length; index++)
                    _presenter.OnChildPresenterRemoved(values[index]);
            }

            public bool Contains(IChildViewModelPresenter item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(IChildViewModelPresenter[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(IChildViewModelPresenter item)
            {
                Should.NotBeNull(item, nameof(item));
                var remove = _list.Remove(item);
                if (remove)
                    _presenter.OnChildPresenterRemoved(item);
                return remove;
            }

            int IComparer<IChildViewModelPresenter>.Compare(IChildViewModelPresenter x1, IChildViewModelPresenter x2)
            {
                return x2.Priority.CompareTo(x1.Priority);
            }

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

            #endregion

            #region Methods

            public IDisposable SuspendNavigation()
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