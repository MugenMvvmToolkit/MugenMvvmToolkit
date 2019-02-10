using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Navigation.Presenters.Results;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter//todo listeners, move callbacks??
    {
        #region Fields

        private readonly PresentersCollection _presenters;

        public const int NavigationPresenterPriority = -1;
        public const int MultiViewModelPresenterPriority = 0;
        public const int WindowPresenterPriority = 1;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(INavigationDispatcher navigationDispatcher, ITracer tracer)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            NavigationDispatcher = navigationDispatcher;
            Tracer = tracer;
            _presenters = new PresentersCollection(this);
            navigationDispatcher.AddListener(_presenters);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected ITracer Tracer { get; }

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

        public IClosingViewModelPresenterResult TryClose(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_presenters.SuspendNavigation())
            {
                var result = TryCloseInternal(metadata);
                if (result == null)
                    return ClosingViewModelPresenterResult.FalseResult;

                return OnClosedInternal(metadata, result);
            }
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            var result = TryRestoreInternal(metadata);
            if (result == null)
                return RestorationViewModelPresenterResult.Unrestored;
            return OnRestoredInternal(metadata, result);
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryShow(this, metadata);
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
            var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
            if (viewModel == null)
                throw ExceptionManager.PresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

            var showingCallback = CreateNavigationCallback(result, NavigationCallbackType.Showing);
            var closeCallback = CreateNavigationCallback(result, NavigationCallbackType.Close);
            AddCallback(viewModel, NavigationInternalMetadata.ShowingCallbacks, showingCallback);
            AddCallback(viewModel, NavigationInternalMetadata.CloseCallbacks, closeCallback);
            return new ViewModelPresenterResult(result.Metadata, showingCallback, closeCallback);
        }

        protected virtual IChildViewModelPresenterResult? TryCloseInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryClose(this, metadata);
                if (operation != null)
                {
                    Trace("close", metadata, presenters[i], operation);
                    return operation;
                }
            }

            var viewModel = metadata.Get(NavigationMetadata.ViewModel);

            if (viewModel != null)
            {
                var closeHandler = viewModel.Metadata.Get(ViewModelMetadata.CloseHandler);
                if (closeHandler != null)
                    return closeHandler(NavigationDispatcher, viewModel, metadata);

                //todo fix wrapperviewmodel
                //                var wrapperViewModel = viewModel.Settings.Metadata.GetData(ViewModelConstants.WrapperViewModel);
                //                if (wrapperViewModel != null)
                //                {
                //                    var ctx = new MetadataContext(metadata);
                //                    ctx.Set(NavigationMetadata.ViewModel, wrapperViewModel);
                //                    return TryCloseInternal(metadata);
                //                }
            }

            return null;
        }

        protected virtual IClosingViewModelPresenterResult OnClosedInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult result)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
            if (viewModel == null)
                throw ExceptionManager.PresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

            var callback = CreateNavigationCallback(result, NavigationCallbackType.Closing);
            Should.BeOfType<INavigationCallback<bool>>(callback, nameof(callback));
            AddCallback(viewModel, NavigationInternalMetadata.ClosingCallbacks, callback);
            return new ClosingViewModelPresenterResult(result.Metadata, (INavigationCallback<bool>)callback);
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] is IRestorableChildViewModelPresenter presenter)
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

        protected virtual IRestorationViewModelPresenterResult OnRestoredInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult result)
        {
            return new RestorationViewModelPresenterResult(result.Metadata, result.NavigationType, true);
        }

        protected virtual void OnChildPresenterAdded(IChildViewModelPresenter presenter)
        {
        }

        protected virtual void OnChildPresenterRemoved(IChildViewModelPresenter presenter)
        {
        }

        protected virtual void OnNavigated(INavigationContext context)
        {
            if (context.NavigationMode.IsClose)
            {
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ShowingCallbacks, context, Default.TrueObject, null, false);
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, context, Default.TrueObject, null, false);
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.CloseCallbacks, context, Default.TrueObject, null, false);
            }
            else
                InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.ShowingCallbacks, context, Default.TrueObject, null, false);
        }

        protected virtual void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            OnNavigationFailed(context, exception, false);
        }

        protected virtual void OnNavigationCanceled(INavigationContext context)
        {
            OnNavigationFailed(context, null, true);
        }

        protected virtual void OnNavigatingCanceled(INavigationContext context)
        {
            if (context.NavigationMode.IsClose)
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, context, Default.FalseObject, null, false);
        }

        protected virtual IReadOnlyList<INavigationCallback> GetCallbacks(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry,
            NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata)
        {
            List<INavigationCallback>? callbacks = null;
            if (callbackType == null)
            {
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.ShowingCallbacks, ref callbacks);
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.ClosingCallbacks, ref callbacks);
                AddEntryCallbacks(navigationEntry, NavigationInternalMetadata.CloseCallbacks, ref callbacks);
            }
            else
            {
                IMetadataContextKey<IList<INavigationCallbackInternal?>?> key = null;
                if (callbackType == NavigationCallbackType.Showing)
                    key = NavigationInternalMetadata.ShowingCallbacks;
                else if (callbackType == NavigationCallbackType.Closing)
                    key = NavigationInternalMetadata.ClosingCallbacks;
                else if (callbackType == NavigationCallbackType.Close)
                    key = NavigationInternalMetadata.CloseCallbacks;
                if (key != null)
                    AddEntryCallbacks(navigationEntry, key, ref callbacks);
            }

            if (callbacks == null)
                return Default.EmptyArray<INavigationCallback>();
            return callbacks;
        }

        protected virtual INavigationCallbackInternal CreateNavigationCallback(IChildViewModelPresenterResult presenterResult, NavigationCallbackType callbackType)
        {
            var serializable = callbackType == NavigationCallbackType.Close && presenterResult.Metadata.Get(NavigationInternalMetadata.IsRestorableCallback);
            return new NavigationCallback(callbackType, presenterResult.NavigationType, serializable, presenterResult.NavigationProvider.Id);
        }

        protected static void AddCallback(IViewModel viewModel, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, INavigationCallbackInternal callback)
        {
            var callbacks = viewModel.Metadata.GetOrAdd(key, (object)null, (object)null, (context, o, arg3) => new List<INavigationCallbackInternal>());
            lock (callback)
            {
                callbacks.Add(callback);
            }
        }

        protected void InvokeCallbacks(IViewModel? viewModel, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, INavigationContext navigationContext, object result, Exception exception, bool canceled)
        {
            if (viewModel == null)//todo trace
                return;
            var callbacks = viewModel.Metadata.Get(key);
            if (callbacks == null)
                return;
            List<INavigationCallbackInternal> toInvoke = null;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationType == navigationContext.NavigationType && callback.NavigationProviderId == navigationContext.NavigationProvider.Id)
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
            }
        }

        private void OnNavigationFailed(INavigationContext context, Exception e, bool canceled)
        {
            if (context.NavigationMode.IsClose)
            {
                InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ClosingCallbacks, context, Default.FalseObject, e, canceled);
                if (!canceled)
                {
                    InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.ShowingCallbacks, context, Default.FalseObject, e, false);
                    InvokeCallbacks(context.ViewModelFrom, NavigationInternalMetadata.CloseCallbacks, context, Default.FalseObject, e, false);
                }
            }
            else
            {
                InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.ShowingCallbacks, context, Default.FalseObject, e, canceled);
                if (context.NavigationMode.IsNew || !canceled)
                {
                    InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.ClosingCallbacks, context, Default.FalseObject, e, canceled);
                    InvokeCallbacks(context.ViewModelTo, NavigationInternalMetadata.CloseCallbacks, context, Default.FalseObject, e, canceled);
                }
            }
        }

        private static void AddEntryCallbacks(INavigationEntry navigationEntry, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, ref List<INavigationCallback>? callbacks)
        {
            var list = navigationEntry.ViewModel.Metadata.Get(key);
            if (list == null)
                return;
            if (callbacks == null)
                callbacks = new List<INavigationCallback>();
            lock (list)
            {
                callbacks.AddRange(list.Where(c => c != null));
            }
        }

        private void Trace(string requestName, IReadOnlyMetadataContext metadata, IChildViewModelPresenter presenter, IHasMetadata<IReadOnlyMetadataContext> hasMetadata)//todo remove all traces
        {
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Info(MessageConstants.TraceViewModelPresenterFormat3, requestName, metadata.Dump() + hasMetadata.Metadata.Dump(), presenter.GetType().FullName);
        }

        #endregion

        #region Nested types

        private sealed class PresentersCollection : ICollection<IChildViewModelPresenter>, IComparer<IChildViewModelPresenter>, ICallbackProviderNavigationDispatcherListener, IDisposable
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

                _presenter.OnNavigated(context);
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

                _presenter.OnNavigationFailed(context, exception);
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

                _presenter.OnNavigationCanceled(context);
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

                _presenter.OnNavigatingCanceled(context);
            }

            public IReadOnlyList<INavigationCallback> GetCallbacks(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata)
            {
                return _presenter.GetCallbacks(navigationDispatcher, navigationEntry, callbackType, metadata);
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