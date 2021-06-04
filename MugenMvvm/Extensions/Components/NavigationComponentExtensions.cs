using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class NavigationComponentExtensions
    {
        public static void OnNavigationEntryAdded(this ItemOrArray<INavigationEntryListener> listeners, INavigationDispatcher navigationDispatcher,
            INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            foreach (var c in listeners)
                c.OnNavigationEntryAdded(navigationDispatcher, navigationEntry, navigationInfo);
        }

        public static void OnNavigationEntryUpdated(this ItemOrArray<INavigationEntryListener> listeners, INavigationDispatcher navigationDispatcher,
            INavigationEntry navigationEntry,
            IHasNavigationInfo? navigationInfo)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            foreach (var c in listeners)
                c.OnNavigationEntryUpdated(navigationDispatcher, navigationEntry, navigationInfo);
        }

        public static void OnNavigationEntryRemoved(this ItemOrArray<INavigationEntryListener> listeners, INavigationDispatcher navigationDispatcher,
            INavigationEntry navigationEntry,
            IHasNavigationInfo? navigationInfo)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationEntry, nameof(navigationEntry));
            foreach (var c in listeners)
                c.OnNavigationEntryRemoved(navigationDispatcher, navigationEntry, navigationInfo);
        }

        public static INavigationCallback? TryAddNavigationCallback(this ItemOrArray<INavigationCallbackManagerComponent> components,
            INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId, NavigationType navigationType, object request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(request, nameof(request));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            foreach (var c in components)
            {
                var callback = c.TryAddNavigationCallback(navigationDispatcher, callbackType, navigationId, navigationType, request, metadata);
                if (callback != null)
                    return callback;
            }

            return null;
        }

        public static ItemOrIReadOnlyList<INavigationCallback> TryGetNavigationCallbacks(this ItemOrArray<INavigationCallbackManagerComponent> components,
            INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetNavigationCallbacks(navigationDispatcher, request, metadata);
            var result = new ItemOrListEditor<INavigationCallback>();
            foreach (var c in components)
                result.AddRange(c.TryGetNavigationCallbacks(navigationDispatcher, request, metadata));

            return result.ToItemOrList();
        }

        public static bool TryInvokeNavigationCallbacks(this ItemOrArray<INavigationCallbackManagerComponent> components, INavigationDispatcher navigationDispatcher,
            NavigationCallbackType callbackType,
            INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var result = false;
            foreach (var c in components)
            {
                if (c.TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext))
                    result = true;
            }

            return result;
        }

        public static bool TryInvokeNavigationCallbacks(this ItemOrArray<INavigationCallbackManagerComponent> components, INavigationDispatcher navigationDispatcher,
            NavigationCallbackType callbackType,
            INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackType, nameof(callbackType));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            var result = false;
            foreach (var c in components)
            {
                if (c.TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext, exception))
                    result = true;
            }

            return result;
        }

        public static bool TryInvokeNavigationCallbacks(this ItemOrArray<INavigationCallbackManagerComponent> components, INavigationDispatcher navigationDispatcher,
            NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(callbackType, nameof(callbackType));
            var result = false;
            foreach (var c in components)
            {
                if (c.TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext, cancellationToken))
                    result = true;
            }

            return result;
        }

        public static INavigationContext? TryGetNavigationContext(this ItemOrArray<INavigationContextProviderComponent> components, INavigationDispatcher navigationDispatcher,
            object? target, INavigationProvider navigationProvider, string navigationId, NavigationType navigationType, NavigationMode navigationMode,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationId, nameof(navigationId));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            foreach (var c in components)
            {
                var context = c.TryGetNavigationContext(navigationDispatcher, target, navigationProvider, navigationId, navigationType, navigationMode, metadata);
                if (context != null)
                    return context;
            }

            return null;
        }

        public static ItemOrIReadOnlyList<INavigationEntry> TryGetNavigationEntries(this ItemOrArray<INavigationEntryProviderComponent> components,
            INavigationDispatcher navigationDispatcher,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetNavigationEntries(navigationDispatcher, metadata);
            var result = new ItemOrListEditor<INavigationEntry>();
            foreach (var c in components)
                result.AddRange(c.TryGetNavigationEntries(navigationDispatcher, metadata));

            return result.ToItemOrList();
        }

        public static ValueTask<bool> CanNavigateAsync(this ItemOrArray<INavigationConditionComponent> components, INavigationDispatcher navigationDispatcher,
            INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            return components.InvokeSequentiallyAsync((navigationDispatcher, navigationContext), cancellationToken, null,
                (component, s, c, _) => component.CanNavigateAsync(s.navigationDispatcher, s.navigationContext, c), true);
        }

        public static async ValueTask<bool> OnNavigatingAsync(this ItemOrArray<INavigationConditionComponent> components, ItemOrArray<INavigationListener> listeners,
            INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var result = true;
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!await c.CanNavigateAsync(navigationDispatcher, navigationContext, cancellationToken).ConfigureAwait(false))
                {
                    result = false;
                    break;
                }
            }

            if (result)
                listeners.OnNavigating(navigationDispatcher, navigationContext);
            return result;
        }

        public static void OnNavigating(this ItemOrArray<INavigationListener> listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            foreach (var t in listeners)
                t.OnNavigating(navigationDispatcher, navigationContext);
        }

        public static void OnNavigated(this ItemOrArray<INavigationListener> listeners, INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            foreach (var c in listeners)
                c.OnNavigated(navigationDispatcher, navigationContext);
        }

        public static void OnNavigationFailed(this ItemOrArray<INavigationErrorListener> listeners, INavigationDispatcher navigationDispatcher,
            INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            foreach (var c in listeners)
                c.OnNavigationFailed(navigationDispatcher, navigationContext, exception);
        }

        public static void OnNavigationCanceled(this ItemOrArray<INavigationErrorListener> listeners, INavigationDispatcher navigationDispatcher,
            INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            foreach (var c in listeners)
                c.OnNavigationCanceled(navigationDispatcher, navigationContext, cancellationToken);
        }
    }
}