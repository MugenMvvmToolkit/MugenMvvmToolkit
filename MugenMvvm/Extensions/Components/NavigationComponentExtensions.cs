using System;
using System.Collections.Generic;
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

        public static void OnNavigationEntryAdded(this INavigationDispatcherEntryListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntryAdded(navigationDispatcher, navigationEntry, navigationInfo);
        }

        public static void OnNavigationEntryUpdated(this INavigationDispatcherEntryListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry,
            IHasNavigationInfo? navigationInfo)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntryUpdated(navigationDispatcher, navigationEntry, navigationInfo);
        }

        public static void OnNavigationEntryRemoved(this INavigationDispatcherEntryListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry,
            IHasNavigationInfo? navigationInfo)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationEntryRemoved(navigationDispatcher, navigationEntry, navigationInfo);
        }

        public static INavigationCallback? TryAddNavigationCallback(this INavigationCallbackManagerComponent[] components,
            INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId, NavigationType navigationType, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(request, nameof(request));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            for (var i = 0; i < components.Length; i++)
            {
                var callback = components[i].TryAddNavigationCallback(navigationDispatcher, callbackType, navigationId, navigationType, request, metadata);
                if (callback != null)
                    return callback;
            }

            return null;
        }

        public static ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> TryGetNavigationCallbacks(this INavigationCallbackManagerComponent[] components,
            INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryGetNavigationCallbacks(navigationDispatcher, request, metadata);
            var result = ItemOrListEditor.Get<INavigationCallback>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetNavigationCallbacks(navigationDispatcher, request, metadata));
            return result.ToItemOrList<IReadOnlyList<INavigationCallback>>();
        }

        public static bool TryInvokeNavigationCallbacks(this INavigationCallbackManagerComponent[] components, INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType,
            INavigationContext navigationContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext))
                    result = true;
            }

            return result;
        }

        public static bool TryInvokeNavigationCallbacks(this INavigationCallbackManagerComponent[] components, INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType,
            INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext, exception))
                    result = true;
            }

            return result;
        }

        public static bool TryInvokeNavigationCallbacks(this INavigationCallbackManagerComponent[] components, INavigationDispatcher navigationDispatcher,
            NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackType, nameof(callbackType));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext, cancellationToken))
                    result = true;
            }

            return result;
        }

        public static INavigationContext? TryGetNavigationContext(this INavigationContextProviderComponent[] components, INavigationDispatcher navigationDispatcher,
            object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType, NavigationMode navigationMode, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            for (var i = 0; i < components.Length; i++)
            {
                var context = components[i].TryGetNavigationContext(navigationDispatcher, target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static ItemOrList<INavigationEntry, IReadOnlyList<INavigationEntry>> TryGetNavigationEntries(this INavigationEntryProviderComponent[] components, INavigationDispatcher navigationDispatcher,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            if (components.Length == 1)
                return components[0].TryGetNavigationEntries(navigationDispatcher, metadata);
            var result = ItemOrListEditor.Get<INavigationEntry>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetNavigationEntries(navigationDispatcher, metadata));
            return result.ToItemOrList<IReadOnlyList<INavigationEntry>>();
        }

        public static async Task<bool> OnNavigatingAsync(this IConditionNavigationDispatcherComponent[] components, INavigationDispatcherNavigatingListener[] listeners,
            INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var result = true;
            for (var i = 0; i < components.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!await components[i].CanNavigateAsync(navigationDispatcher, navigationContext, cancellationToken).ConfigureAwait(false))
                {
                    result = false;
                    break;
                }
            }

            if (result)
                listeners.OnNavigating(navigationDispatcher, navigationContext);
            return result;
        }

        public static void OnNavigating(this INavigationDispatcherNavigatingListener[] listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigating(navigationDispatcher, navigationContext);
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
    }
}