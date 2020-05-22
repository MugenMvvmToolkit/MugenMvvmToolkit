using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class NavigationComponentExtensions
    {
        #region Methods

        public static void OnNavigationEntryAdded(this INavigationDispatcherEntryListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntryAdded(navigationDispatcher, navigationEntry, navigationContext);
        }

        public static void OnNavigationEntryUpdated(this INavigationDispatcherEntryListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry,
            INavigationContext? navigationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntryUpdated(navigationDispatcher, navigationEntry, navigationContext);
        }

        public static void OnNavigationEntryRemoved(this INavigationDispatcherEntryListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry,
            INavigationContext? navigationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntryRemoved(navigationDispatcher, navigationEntry, navigationContext);
        }

        public static INavigationCallback? TryAddNavigationCallback<TRequest>(this INavigationCallbackManagerComponent[] components,
            NavigationCallbackType callbackType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(callbackType, nameof(callbackType));
            for (var i = 0; i < components.Length; i++)
            {
                var callback = components[i].TryAddNavigationCallback(callbackType, request, metadata);
                if (callback != null)
                    return callback;
            }

            return null;
        }

        public static IReadOnlyList<INavigationCallback>? TryGetNavigationCallbacks<TTarget>(this INavigationCallbackManagerComponent[] components, [DisallowNull] in TTarget target,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<INavigationCallback> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetNavigationCallbacks(target, metadata));
            return result.List;
        }

        public static bool TryInvokeNavigationCallbacks<TRequest>(this INavigationCallbackManagerComponent[] components,
            NavigationCallbackType callbackType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(callbackType, nameof(callbackType));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryInvokeNavigationCallbacks(callbackType, request, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryInvokeNavigationCallbacks<TRequest>(this INavigationCallbackManagerComponent[] components,
            NavigationCallbackType callbackType, [DisallowNull] in TRequest request, Exception exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(exception, nameof(exception));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryInvokeNavigationCallbacks(callbackType, request, exception, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryInvokeNavigationCallbacks<TRequest>(this INavigationCallbackManagerComponent[] components,
            NavigationCallbackType callbackType, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(callbackType, nameof(callbackType));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryInvokeNavigationCallbacks(callbackType, request, cancellationToken, metadata))
                    result = true;
            }

            return result;
        }

        public static INavigationContext? TryGetNavigationContext(this INavigationContextProviderComponent[] components, INavigationProvider navigationProvider, string navigationId,
            NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            for (var i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetNavigationContext(navigationProvider, navigationId, navigationType, navigationMode, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static IReadOnlyList<INavigationEntry>? TryGetNavigationEntries(this INavigationEntryProviderComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<INavigationEntry> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetNavigationEntries(metadata));
            return result.List;
        }

        public static Task<bool> OnNavigatingAsync(this INavigationDispatcherNavigatingListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            if (listeners.Length == 0)
                return Default.TrueTask;
            if (listeners.Length == 1)
                return listeners[0].OnNavigatingAsync(navigationDispatcher, navigationContext, cancellationToken) ?? Default.TrueTask;
            return new NavigatingResult(navigationDispatcher, listeners, navigationContext, cancellationToken).Task;
        }

        public static void OnNavigated(this INavigationDispatcherNavigatedListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigated(navigationDispatcher, navigationContext);
        }

        public static void OnNavigationFailed(this INavigationDispatcherErrorListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationFailed(navigationDispatcher, navigationContext, exception);
        }

        public static void OnNavigationCanceled(this INavigationDispatcherErrorListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationCanceled(navigationDispatcher, navigationContext, cancellationToken);
        }

        #endregion

        #region Nested types

        private sealed class NavigatingResult : TaskCompletionSource<bool>
        {
            #region Fields

            private readonly CancellationToken _cancellationToken;

            private readonly INavigationDispatcherNavigatingListener[] _components;
            private readonly INavigationDispatcher _dispatcher;
            private readonly INavigationContext _navigationContext;
            private int _index;

            #endregion

            #region Constructors

            public NavigatingResult(INavigationDispatcher dispatcher, INavigationDispatcherNavigatingListener[] components, INavigationContext navigationContext, CancellationToken cancellationToken)
            {
                _dispatcher = dispatcher;
                _components = components;
                _navigationContext = navigationContext;
                _cancellationToken = cancellationToken;
                OnExecuted(Default.TrueTask);
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

                    if (_index >= _components.Length)
                    {
                        SetResult(true, null, false);
                        return;
                    }

                    if (_cancellationToken.IsCancellationRequested)
                    {
                        SetResult(false, null, true);
                        return;
                    }

                    var resultTask = _components[_index++].OnNavigatingAsync(_dispatcher, _navigationContext, _cancellationToken) ?? Default.TrueTask;
                    resultTask.ContinueWith((t, state) => ((NavigatingResult)state).OnExecuted(t), this, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception e)
                {
                    SetResult(false, e, false);
                }
            }

            private void SetResult(bool result, Exception? exception, bool canceled)
            {
                if (exception != null)
                    this.TrySetExceptionEx(exception);
                else if (canceled)
                    TrySetCanceled();
                else
                    TrySetResult(result);
            }

            #endregion
        }

        #endregion
    }
}