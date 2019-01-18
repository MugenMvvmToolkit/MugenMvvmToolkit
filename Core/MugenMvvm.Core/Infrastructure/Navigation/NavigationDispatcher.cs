﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationDispatcher : HasListenersBase<INavigationDispatcherListener>, INavigationDispatcher, IApplicationStateDispatcherListener
    {
        #region Fields

        protected readonly Dictionary<NavigationType, List<WeakNavigationEntry>> OpenedViewModels;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcher(IApplicationStateDispatcher applicationStateDispatcher, ITracer tracer)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            Should.NotBeNull(applicationStateDispatcher, nameof(applicationStateDispatcher));
            Tracer = tracer;
            OpenedViewModels = new Dictionary<NavigationType, List<WeakNavigationEntry>>();
            applicationStateDispatcher.AddListener(this);
        }

        #endregion

        #region Properties

        protected ITracer Tracer { get; }

        #endregion

        #region Implementation of interfaces

        void IApplicationStateDispatcherListener.OnStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
        {
            OnApplicationStateChanged(dispatcher, oldState, newState, metadata);
        }

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return GetNavigationEntriesInternal(type, metadata);
        }

        public INavigatingResult OnNavigating(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            Trace(nameof(OnNavigating), context);
            return OnNavigatingInternal(context);
        }

        public void OnNavigated(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            HandleOpenedViewModels(context);
            OnNavigatedInternal(context);
            Trace(nameof(OnNavigated), context);
        }

        public void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(exception, nameof(exception));
            OnNavigationFailedInternal(context, exception);
            Trace(nameof(OnNavigationFailed), context);
        }

        public void OnNavigationCanceled(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            OnNavigationCanceledInternal(context);
            Trace(nameof(OnNavigationCanceled), context);
        }

        #endregion

        #region Methods

        protected virtual void HandleOpenedViewModels(INavigationContext context)
        {
            var viewModelFrom = context.Metadata.Get(NavigationInternalMetadata.ViewModelFromNavigationType, context.NavigationType) == context.NavigationType
                ? context.ViewModelFrom
                : null;
            var viewModelTo = context.Metadata.Get(NavigationInternalMetadata.ViewModelToNavigationType, context.NavigationType) == context.NavigationType
                ? context.ViewModelTo
                : null;

            lock (OpenedViewModels)
            {
                if (!OpenedViewModels.TryGetValue(context.NavigationType, out var list))
                {
                    list = new List<WeakNavigationEntry>();
                    OpenedViewModels[context.NavigationType] = list;
                }

                if (viewModelTo != null && (context.NavigationMode == NavigationMode.Refresh || context.NavigationMode == NavigationMode.Back ||
                                            context.NavigationMode == NavigationMode.New))
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
                        viewModelRef = new WeakNavigationEntry(this, viewModelTo, context.NavigationProvider, context.NavigationType);
                    list.Add(viewModelRef);
                }

                if (viewModelFrom != null && context.NavigationMode.IsClose())
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

        protected virtual IReadOnlyList<INavigationEntry> GetNavigationEntriesInternal(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            lock (OpenedViewModels)
            {
                List<INavigationEntry>? result = null;
                if (type == null)
                {
                    var array = OpenedViewModels.Keys.ToArray();
                    for (var i = 0; i < array.Length; i++)
                        GetOpenedViewModelsInternal(array[i], ref result);
                }
                else
                    GetOpenedViewModelsInternal(type, ref result);

                if (result == null)
                    return Default.EmptyArray<INavigationEntry>();
                return result;
            }
        }

        protected virtual INavigatingResult OnNavigatingInternal(INavigationContext context)
        {
            var listeners = GetListenersInternal()?.Where(listener => listener != null).ToArray() ?? Default.EmptyArray<INavigationDispatcherListener>();
            var invoker = new NavigatingResult(this, listeners, context);
            return invoker;
        }

        protected virtual void OnNavigatedInternal(INavigationContext context)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnNavigated(context);
        }

        protected virtual void OnNavigationFailedInternal(INavigationContext context, Exception exception)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnNavigationFailed(context, exception);
        }

        protected virtual void OnNavigationCanceledInternal(INavigationContext context)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnNavigationCanceled(context);
        }

        protected virtual void OnNavigatingCanceledInternal(INavigationContext context)
        {
            var listeners = GetListenersInternal();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnNavigatingCanceled(context);
        }

        protected virtual IReadOnlyList<INavigationCallback> GetCallbacksInternal(INavigationEntry navigationEntry, NavigationCallbackType? callbackType,
            IReadOnlyMetadataContext metadata)
        {
            List<INavigationCallback>? callbacks = null;
            if (callbackType == null)
            {
                AddCallbacks(navigationEntry, NavigationInternalMetadata.ShowingCallbacks, ref callbacks);
                AddCallbacks(navigationEntry, NavigationInternalMetadata.ClosingCallbacks, ref callbacks);
                AddCallbacks(navigationEntry, NavigationInternalMetadata.CloseCallbacks, ref callbacks);
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
                    AddCallbacks(navigationEntry, key, ref callbacks);
            }

            if (callbacks == null)
                return Default.EmptyArray<INavigationCallback>();
            return callbacks;
        }

        protected virtual void OnApplicationStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
        {
            var entries = new List<WeakNavigationEntry>();
            lock (OpenedViewModels)
            {
                foreach (var pair in OpenedViewModels)
                {
                    entries.AddRange(pair.Value);
                }
            }

            for (int i = 0; i < entries.Count; i++)
                entries[i].OnApplicationStateChanged(dispatcher, oldState, newState, metadata);
        }

        private void GetOpenedViewModelsInternal(NavigationType type, ref List<INavigationEntry>? result)
        {
            if (!OpenedViewModels.TryGetValue(type, out var list))
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
                OpenedViewModels.Remove(type);
        }

        private static void AddCallbacks(INavigationEntry navigationEntry, IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, ref List<INavigationCallback>? callbacks)
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

        private void Trace(string navigationName, INavigationContext context)
        {
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Info(MessageConstants.TraceNavigationFormat5, navigationName, context.NavigationMode, context.ViewModelFrom, context.ViewModelTo, context.NavigationType);
        }

        #endregion

        #region Nested types

        protected sealed class NavigatingResult : TaskCompletionSource<bool>, INavigatingResult
        {
            #region Fields

            private readonly INavigationContext _context;
            private readonly NavigationDispatcher _dispatcher;
            private readonly INavigationDispatcherListener[] _listeners;
            private Action<INavigationDispatcher, INavigationContext, Exception?>? _canceledCallback;
            private Func<INavigationDispatcher, INavigationContext, bool> _completeNavigationCallback;
            private int _index;

            #endregion

            #region Constructors

            public NavigatingResult(NavigationDispatcher dispatcher, INavigationDispatcherListener[] listeners, INavigationContext context)
            {
                _dispatcher = dispatcher;
                _listeners = listeners;
                _context = context;
                OnExecuted(Default.TrueTask);
            }

            #endregion

            #region Implementation of interfaces

            public Task<bool> GetResultAsync()
            {
                return Task;
            }

            public void CompleteNavigation(Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
                Action<INavigationDispatcher, INavigationContext, Exception?>? canceledCallback = null)
            {
                Should.NotBeNull(completeNavigationCallback, nameof(completeNavigationCallback));
                if (Interlocked.Exchange(ref _completeNavigationCallback, completeNavigationCallback) != null)
                    throw ExceptionManager.NavigatingResultHasCallback();
                _canceledCallback = canceledCallback;
                Task.ContinueWith(InvokeCompletedCallback, this, TaskContinuationOptions.ExecuteSynchronously);
            }

            #endregion

            #region Methods

            private void OnExecuted(Task<bool> task)
            {
                try
                {
                    if (task.IsCanceled)
                    {
                        SetResult(false, null, true);
                        return;
                    }

                    if (!task.Result)
                    {
                        SetResult(false, null, false);
                        return;
                    }

                    if (_index >= _listeners.Length)
                    {
                        SetResult(true, null, false);
                        return;
                    }

                    var resultTask = _listeners[_index].OnNavigatingAsync(_context);
                    ++_index;
                    resultTask.ContinueWith(OnExecuted, this, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception e)
                {
                    SetResult(false, e, false);
                }
            }

            private void SetResult(bool result, Exception exception, bool canceled)
            {
                if (exception != null)
                    this.TrySetExceptionEx(exception);
                else if (canceled)
                    TrySetCanceled();
                else
                {
                    TrySetResult(result);
                    if (!result)
                        _dispatcher.OnNavigatingCanceledInternal(_context);
                }
            }

            private void InvokeCompletedCallback(Task<bool> task)
            {
                try
                {
                    if (task.IsCanceled)
                    {
                        _canceledCallback?.Invoke(_dispatcher, _context, null);
                        _dispatcher.OnNavigationCanceled(_context);
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        _canceledCallback?.Invoke(_dispatcher, _context, task.Exception);
                        _dispatcher.OnNavigationFailed(_context, task.Exception);
                        return;
                    }

                    if (task.Result)
                    {
                        if (_completeNavigationCallback(_dispatcher, _context))
                            _dispatcher.OnNavigated(_context);
                    }
                    else
                    {
                        _canceledCallback?.Invoke(_dispatcher, _context, null);
                        _dispatcher.OnNavigationCanceled(_context);
                    }
                }
                catch (Exception e)
                {
                    _canceledCallback?.Invoke(_dispatcher, _context, e);
                    _dispatcher.OnNavigationFailed(_context, e);
                }
                finally
                {
                    _canceledCallback = null;
                    _completeNavigationCallback = (dispatcher, context) => false;
                }
            }

            private static void InvokeCompletedCallback(Task<bool> task, object state)
            {
                ((NavigatingResult)state).InvokeCompletedCallback(task);
            }

            private static void OnExecuted(Task<bool> task, object state)
            {
                ((NavigatingResult)state).OnExecuted(task);
            }

            #endregion
        }

        protected sealed class WeakNavigationEntry
        {
            #region Fields

            private readonly NavigationDispatcher _dispatcher;
            private readonly DateTime _date;
            private readonly WeakReference _viewModelReference;

            #endregion

            #region Constructors

            public WeakNavigationEntry(NavigationDispatcher dispatcher, IViewModel viewModel, object provider, NavigationType navigationType)
            {
                _dispatcher = dispatcher;
                NavigationType = navigationType;
                NavigationProvider = provider;
                _viewModelReference = MugenExtensions.GetWeakReference(viewModel);
                _date = DateTime.UtcNow;
            }

            #endregion

            #region Properties

            public IViewModel? ViewModel => (IViewModel)_viewModelReference.Target;

            public object NavigationProvider { get; }

            public NavigationType NavigationType { get; }

            #endregion

            #region Methods

            public void OnApplicationStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
            {
                if (!(NavigationProvider is IApplicationStateSupportedNavigationProvider navigationProvider))
                    return;
                var viewModel = ViewModel;
                if (viewModel == null)
                    return;
                if (!navigationProvider.IsSupported(viewModel, oldState, newState, metadata))
                    return;

                var ctx = navigationProvider.TryCreateApplicationStateContext(viewModel, oldState, newState, metadata);
                if (ctx == null)
                {
                    if (newState == ApplicationState.Active)
                        ctx = new NavigationContext(navigationProvider, NavigationType, NavigationMode.Foreground, null, viewModel, metadata);
                    else if (newState == ApplicationState.Background)
                        ctx = new NavigationContext(navigationProvider, NavigationType, NavigationMode.Background, viewModel, null, metadata);
                }

                if (ctx != null)
                    _dispatcher.OnNavigated(ctx);
            }

            public INavigationEntry? ToNavigationEntry()
            {
                var viewModel = ViewModel;
                var provider = NavigationProvider;
                if (viewModel == null)
                    return null;
                return new NavigationEntry(_dispatcher, NavigationType, viewModel, _date, provider);
            }

            #endregion
        }

        protected sealed class NavigationEntry : INavigationEntry
        {
            #region Fields

            private readonly NavigationDispatcher _navigationDispatcher;

            #endregion

            #region Constructors

            public NavigationEntry(NavigationDispatcher navigationDispatcher, NavigationType type, IViewModel viewModel, DateTime date, object provider)
            {
                Should.NotBeNull(type, nameof(type));
                Should.NotBeNull(viewModel, nameof(viewModel));
                Should.NotBeNull(provider, nameof(provider));
                _navigationDispatcher = navigationDispatcher;
                NavigationDate = date;
                NavigationType = type;
                NavigationProvider = provider;
                ViewModel = viewModel;
            }

            #endregion

            #region Properties

            public DateTime NavigationDate { get; }

            public NavigationType NavigationType { get; }

            public object NavigationProvider { get; }

            public IViewModel ViewModel { get; }

            #endregion

            #region Implementation of interfaces

            public IReadOnlyList<INavigationCallback> GetCallbacks(NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata)
            {
                Should.NotBeNull(callbackType, nameof(callbackType));
                Should.NotBeNull(metadata, nameof(metadata));
                return _navigationDispatcher.GetCallbacksInternal(this, callbackType, metadata);
            }

            #endregion
        }

        #endregion
    }
}