using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;

namespace MugenMvvm.Presenters.Components
{
    public sealed class ViewModelCallbackManagerComponent : AttachableComponentBase<IPresenter>, IPresenterComponent, ICloseablePresenterComponent, IHasPriority
    {
        #region Fields

        private readonly NavigationDispatcherListener _dispatcherListener;
        private INavigationDispatcher? _navigationDispatcher;

        private static readonly IMetadataContextKey<List<NavigationCallback?>> ShowingCallbacks = GetBuilder<List<NavigationCallback?>>(nameof(ShowingCallbacks))
            .Build();

        private static readonly IMetadataContextKey<List<NavigationCallback?>> ClosingCallbacks = GetBuilder<List<NavigationCallback?>>(nameof(ClosingCallbacks))
            .Build();

        private static readonly IMetadataContextKey<List<NavigationCallback?>> CloseCallbacks = GetBuilder<List<NavigationCallback?>>(nameof(CloseCallbacks))
            .Serializable(CanSerializeCloseCallbacks)
            .SerializableConverter(SerializeCloseCallbacks, (key, o, arg3) => o)
            .Build();

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelCallbackManagerComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
            _dispatcherListener = new NavigationDispatcherListener(this);
        }

        #endregion

        #region Properties

        public bool IsSerializable { get; set; }

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IPresenterResult> TryClose(IMetadataContext metadata)
        {
            _dispatcherListener.BeginSuspend();
            var components = Owner.GetComponents();
            try
            {
                var results = new List<IPresenterResult>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (!(components[i] is ICloseablePresenterComponent presenter) || presenter.GetPriority(Owner) >= Priority
                                                                                   || !Owner.CanClose(presenter, results, metadata))
                        continue;

                    var operations = presenter.TryClose(metadata);
                    if (operations != null)
                        results.AddRange(operations);
                }

                for (var i = 0; i < results.Count; i++)
                {
                    var presenterResult = results[i];
                    var callback = AddCallback(presenterResult, NavigationCallbackType.Closing);
                    if (callback != null)
                        presenterResult.Metadata.Set(NavigationInternalMetadata.ClosingCallback, callback);
                }

                return results;
            }
            finally
            {
                _dispatcherListener.EndSuspend();
            }
        }

        public IPresenterResult? TryShow(IMetadataContext metadata)
        {
            _dispatcherListener.BeginSuspend();
            var components = Owner.GetComponents();
            try
            {
                for (var i = 0; i < components.Length; i++)
                {
                    if (!(components[i] is IPresenterComponent presenter) || presenter.GetPriority(Owner) >= Priority
                                                                          || !Owner.CanShow(presenter, metadata))
                        continue;

                    var result = presenter.TryShow(metadata);
                    if (result != null)
                    {
                        var callback = AddCallback(result, NavigationCallbackType.Showing);
                        if (callback != null)
                            result.Metadata.Set(NavigationInternalMetadata.ShowingCallback, callback);
                        callback = AddCallback(result, NavigationCallbackType.Close);
                        if (callback != null)
                            result.Metadata.Set(NavigationInternalMetadata.CloseCallback, callback);
                        return result;
                    }
                }

                return null;
            }
            finally
            {
                _dispatcherListener.EndSuspend();
            }
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            if (_navigationDispatcher == null)
                _navigationDispatcher = MugenService.NavigationDispatcher;
            _navigationDispatcher.AddComponent(_dispatcherListener);
        }

        protected override void OnDetachedInternal(IPresenter owner, IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher?.RemoveComponent(_dispatcherListener);
            _navigationDispatcher = null;
        }

        private INavigationCallback<bool>? AddCallback(IPresenterResult presenterResult, NavigationCallbackType callbackType)
        {
            var viewModel = presenterResult.Metadata.Get(NavigationMetadata.ViewModel);
            if (viewModel == null)
                return null;

            var serializable = IsSerializable && callbackType == NavigationCallbackType.Close && presenterResult.Metadata.Get(NavigationInternalMetadata.IsRestorableCallback);
            var callback = new NavigationCallback(callbackType, presenterResult.NavigationType, serializable, presenterResult.NavigationOperationId);
            var key = GetKeyByCallback(callbackType);

            var callbacks = viewModel.Metadata.GetOrAdd(key, (object?)null, (context, _) => new List<NavigationCallback?>());
            lock (callback)
            {
                callbacks.Add(callback);
            }

            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IViewModelCallbackManagerListener)?.OnCallbackAdded(callback, viewModel, presenterResult.Metadata);
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

        private void InvokeCallbacks(INavigationContext navigationContext, IMetadataContextKey<List<NavigationCallback?>> key, bool result, Exception? exception, bool canceled)
        {
            var vm = navigationContext.Metadata.Get(NavigationMetadata.ViewModel);
            var callbacks = vm?.Metadata.Get(key);
            if (callbacks == null)
                return;

            List<NavigationCallback>? toInvoke = null;
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
                                toInvoke = new List<NavigationCallback>();
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

                var components = Owner.GetComponents();
                for (var j = 0; j < components.Length; i++)
                    (components[j] as IViewModelCallbackManagerListener)?.OnCallbackExecuted(callback, vm!, navigationContext);
            }
        }

        private static IMetadataContextKey<List<NavigationCallback?>> GetKeyByCallback(NavigationCallbackType callbackType)
        {
            if (callbackType == NavigationCallbackType.Showing)
                return ShowingCallbacks;
            if (callbackType == NavigationCallbackType.Closing)
                return ClosingCallbacks;
            if (callbackType == NavigationCallbackType.Close)
                return CloseCallbacks;
            ExceptionManager.ThrowEnumOutOfRange(nameof(callbackType), callbackType);
            return null;
        }

        private static void AddEntryCallbacks(INavigationEntry navigationEntry, IMetadataContextKey<List<NavigationCallback?>> key, ref List<INavigationCallback>? callbacks)
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

        private static bool CanSerializeCloseCallbacks(IMetadataContextKey<List<NavigationCallback?>> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<NavigationCallback>?)value;
            return callbacks != null && callbacks.Any(callback => callback != null && callback.IsSerializable);
        }

        private static object? SerializeCloseCallbacks(IMetadataContextKey<List<NavigationCallback?>> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<NavigationCallback>?)value;
            if (callbacks == null)
                return null;
            lock (callbacks)
            {
                return callbacks.Where(callback => callback.IsSerializable).ToList();
            }
        }

        #endregion

        #region Nested types

        private sealed class NavigationDispatcherListener : INavigationDispatcherNavigatedListener, INavigationDispatcherErrorListener, INavigationCallbackProviderComponent, IHasPriority
        {
            #region Fields

            private readonly ViewModelCallbackManagerComponent _callbackManager;
            private readonly List<KeyValuePair<INavigationContext, object?>> _suspendedEvents;
            private int _suspendCount;

            #endregion

            #region Constructors

            public NavigationDispatcherListener(ViewModelCallbackManagerComponent callbackManager)
            {
                _callbackManager = callbackManager;
                _suspendedEvents = new List<KeyValuePair<INavigationContext, object?>>();
            }

            #endregion

            #region Properties

            public int Priority => NavigationComponentPriority.CallbackProvider;

            #endregion

            #region Implementation of interfaces

            public IReadOnlyList<INavigationCallback> TryGetCallbacks(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata = null)
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

                _callbackManager.OnNavigationFailed(navigationContext, exception, false);
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

                _callbackManager.OnNavigationFailed(navigationContext, null, true);
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

            public void BeginSuspend()
            {
                lock (_suspendedEvents)
                {
                    ++_suspendCount;
                }
            }

            public void EndSuspend()
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
                }
            }

            #endregion
        }

        #endregion
    }
}