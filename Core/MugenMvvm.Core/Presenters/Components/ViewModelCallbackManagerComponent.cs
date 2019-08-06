using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelCallbackManagerComponent : AttachableComponentBase<IPresenter>, IPresenterShowListener, IPresenterCloseListener
    {
        #region Fields

        private readonly NavigationDispatcherListener _dispatcherListener;
        private readonly INavigationDispatcher _navigationDispatcher;

        private static readonly IMetadataContextKey<List<NavigationCallbackInternal?>?> ShowingCallbacks = GetBuilder<List<NavigationCallbackInternal?>?>(nameof(ShowingCallbacks))
            .Build();

        private static readonly IMetadataContextKey<List<NavigationCallbackInternal?>?> ClosingCallbacks = GetBuilder<List<NavigationCallbackInternal?>?>(nameof(ClosingCallbacks))
            .Build();

        private static readonly IMetadataContextKey<List<NavigationCallbackInternal?>?> CloseCallbacks = GetBuilder<List<NavigationCallbackInternal?>?>(nameof(CloseCallbacks))
            .Serializable(CanSerializeCloseCallbacks)
            .SerializableConverter(SerializeCloseCallbacks, DeserializeCloseCallbacks)
            .Build();

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelCallbackManagerComponent(INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            _navigationDispatcher = navigationDispatcher;
            _dispatcherListener = new NavigationDispatcherListener(this);
        }

        #endregion

        #region Properties

        public bool IsSerializable { get; set; }

        public int NavigationDispatcherListenerPriority { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnClosing(IPresenter presenter, string operationId, IMetadataContext metadata)
        {
            _dispatcherListener.BeginSuspend(operationId);
        }

        public void OnClosed(IPresenter presenter, string operationId, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata)
        {
            for (var i = 0; i < results.Count; i++)
            {
                var presenterResult = results[i];
                presenterResult.Metadata.Set(NavigationInternalMetadata.ClosingCallback, AddCallback(presenterResult, NavigationCallbackType.Closing));
            }

            _dispatcherListener.EndSuspend(operationId);
        }

        public void OnCloseError(IPresenter presenter, string operationId, Exception exception, IMetadataContext metadata)
        {
            _dispatcherListener.EndSuspend(operationId);
        }

        public int GetPriority(object source)
        {
            return Priority;
        }

        public void OnShowing(IPresenter presenter, string operationId, IMetadataContext metadata)
        {
            _dispatcherListener.BeginSuspend(operationId);
        }

        public void OnShown(IPresenter presenter, string operationId, IPresenterResult result, IMetadataContext metadata)
        {
            result.Metadata.Set(NavigationInternalMetadata.ShowingCallback, AddCallback(result, NavigationCallbackType.Showing));
            result.Metadata.Set(NavigationInternalMetadata.CloseCallback, AddCallback(result, NavigationCallbackType.Close));
            _dispatcherListener.EndSuspend(operationId);
        }

        public void OnShowError(IPresenter presenter, string operationId, Exception exception, IMetadataContext metadata)
        {
            _dispatcherListener.EndSuspend(operationId);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher.AddComponent(_dispatcherListener);
        }

        protected override void OnDetachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher.RemoveComponent(_dispatcherListener);
        }

        private INavigationCallback<bool>? AddCallback(IPresenterResult presenterResult, NavigationCallbackType callbackType)
        {
            var viewModel = presenterResult.Metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;

            var serializable = IsSerializable && callbackType == NavigationCallbackType.Close && presenterResult.Metadata.Get(NavigationInternalMetadata.IsRestorableCallback);
            var callback = new NavigationCallbackInternal(callbackType, presenterResult.NavigationType, serializable, presenterResult.NavigationOperationId);
            var key = GetKeyByCallback(callbackType);

            var callbacks = viewModel.Metadata.GetOrAdd(key!, (object?) null, (object?) null, (context, o, arg3) => new List<NavigationCallbackInternal?>())!;
            lock (callback)
            {
                callbacks.Add(callback);
            }

            OnCallbackAdded(callback, viewModel, presenterResult.Metadata);
            return callback;
        }

        private void OnNavigatedInternal(INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext, ShowingCallbacks!, true, null, false);
                InvokeCallbacks(navigationContext, ClosingCallbacks!, true, null, false);
                InvokeCallbacks(navigationContext, CloseCallbacks, true, null, false);
            }
            else
                InvokeCallbacks(navigationContext, ShowingCallbacks, true, null, false);
        }

        private void OnNavigationFailedInternal(INavigationContext navigationContext, Exception exception)
        {
            OnNavigationFailed(navigationContext, exception, false);
        }

        private void OnNavigationCanceledInternal(INavigationContext navigationContext)
        {
            OnNavigationFailed(navigationContext, null, true);
        }

        private void OnNavigationFailed(INavigationContext navigationContext, Exception? e, bool canceled)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext, ClosingCallbacks!, false, e, canceled);
                if (!canceled)
                {
                    InvokeCallbacks(navigationContext, ShowingCallbacks!, false, e, false);
                    InvokeCallbacks(navigationContext, CloseCallbacks, false, e, false);
                }
            }
            else
            {
                InvokeCallbacks(navigationContext, ShowingCallbacks!, false, e, canceled);
                if (navigationContext.NavigationMode.IsNew || !canceled)
                {
                    InvokeCallbacks(navigationContext, ClosingCallbacks!, false, e, canceled);
                    InvokeCallbacks(navigationContext, CloseCallbacks, false, e, canceled);
                }
            }
        }

        private void InvokeCallbacks(INavigationContext navigationContext, IMetadataContextKey<List<NavigationCallbackInternal?>?> key, bool result, Exception? exception,
            bool canceled)
        {
            var vm = navigationContext.Metadata.Get(NavigationMetadata.ViewModel);
            var callbacks = vm?.Metadata.Get(key);
            if (callbacks == null)
                return;

            List<NavigationCallbackInternal>? toInvoke = null;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationOperationId == navigationContext.NavigationOperationId)
                    {
                        if (callback != null)
                        {
                            if (toInvoke == null)
                                toInvoke = new List<NavigationCallbackInternal>();
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
                    callback.SetException(exception);
                else if (canceled)
                    callback.SetCanceled();
                else
                    callback.SetResult(result);
                OnCallbackExecuted(callback, vm!, navigationContext);
            }
        }

        private void OnCallbackAdded(INavigationCallback callback, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IViewModelCallbackManagerListener)?.OnCallbackAdded(callback, viewModel, metadata);
        }

        private void OnCallbackExecuted(INavigationCallback callback, IViewModelBase viewModel, INavigationContext navigationContext)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IViewModelCallbackManagerListener)?.OnCallbackExecuted(callback, viewModel, navigationContext);
        }

        private static IMetadataContextKey<List<NavigationCallbackInternal?>?>? GetKeyByCallback(NavigationCallbackType callbackType)
        {
            if (callbackType == NavigationCallbackType.Showing)
                return ShowingCallbacks!;
            if (callbackType == NavigationCallbackType.Closing)
                return ClosingCallbacks!;
            if (callbackType == NavigationCallbackType.Close)
                return CloseCallbacks;
            return null;
        }

        private static void AddEntryCallbacks(INavigationEntry navigationEntry, IMetadataContextKey<List<NavigationCallbackInternal?>?> key,
            ref List<INavigationCallback>? callbacks)
        {
            var list = navigationEntry.Metadata.Get(NavigationMetadata.ViewModel)?.Metadata.Get(key);
            if (list == null)
                return;
            if (callbacks == null)
                callbacks = new List<INavigationCallback>();
            lock (list)
            {
                callbacks.AddRange(list.Where(c => c != null)!);
            }
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ViewModelCallbackManagerComponent), name);
        }

        private static bool CanSerializeCloseCallbacks(IMetadataContextKey<List<NavigationCallbackInternal?>?> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<NavigationCallbackInternal>?) value;
            return callbacks != null && callbacks.Any(callback => callback != null && callback.IsSerializable);
        }

        private static object? SerializeCloseCallbacks(IMetadataContextKey<List<NavigationCallbackInternal?>?> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<NavigationCallbackInternal>?) value;
            if (callbacks == null)
                return null;
            lock (callbacks)
            {
                return callbacks.Where(callback => callback.IsSerializable).ToList();
            }
        }

        private static object? DeserializeCloseCallbacks(IMetadataContextKey<List<NavigationCallbackInternal?>?> key, object? value, ISerializationContext context)
        {
            return value;
        }

        #endregion

        #region Nested types

        private sealed class NavigationDispatcherListener : INavigationDispatcherNavigatedListener, INavigationDispatcherErrorListener, INavigationCallbackProviderComponent
        {
            #region Fields

            private readonly ViewModelCallbackManagerComponent _callbackManager;
            private readonly HashSet<string> _operations;
            private readonly List<KeyValuePair<INavigationContext, object?>> _suspendedEvents;
            private int _suspendCount;

            #endregion

            #region Constructors

            public NavigationDispatcherListener(ViewModelCallbackManagerComponent callbackManager)
            {
                _callbackManager = callbackManager;
                _suspendedEvents = new List<KeyValuePair<INavigationContext, object?>>();
                _operations = new HashSet<string>();
            }

            #endregion

            #region Implementation of interfaces

            public IReadOnlyList<INavigationCallback> GetCallbacks(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
            {
                List<INavigationCallback>? callbacks = null;
                AddEntryCallbacks(navigationEntry, ShowingCallbacks, ref callbacks);
                AddEntryCallbacks(navigationEntry, ClosingCallbacks, ref callbacks);
                AddEntryCallbacks(navigationEntry, CloseCallbacks, ref callbacks);

                if (callbacks == null)
                    return Default.EmptyArray<INavigationCallback>();
                return callbacks;
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

            public int GetPriority(object source)
            {
                return _callbackManager.NavigationDispatcherListenerPriority;
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

            #endregion

            #region Methods

            public void BeginSuspend(string id)
            {
                lock (_suspendedEvents)
                {
                    if (_operations.Add(id))
                        ++_suspendCount;
                }
            }

            public void EndSuspend(string id)
            {
                KeyValuePair<INavigationContext, object?>[] events;
                lock (_suspendedEvents)
                {
                    if (!_operations.Remove(id))
                        return;

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
                }
            }

            #endregion
        }

        #endregion
    }
}