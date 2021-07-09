using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationCallbackManager : INavigationCallbackManagerComponent, IHasPriority
    {
        private readonly IAttachedValueManager? _attachedValueManager;

        public NavigationCallbackManager(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        public int Priority { get; init; } = NavigationComponentPriority.CallbackManager;

        public INavigationCallback? TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId,
            NavigationType navigationType, object request, IReadOnlyMetadataContext? metadata)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return null;

            if (request is IHasTarget<object?> hasTarget && hasTarget.Target != null)
                request = hasTarget.Target;

            var targetMetadata = (IMetadataContext?)GetTargetMetadata(request, false);
            if (targetMetadata == null)
                return null;

            var callbacks = targetMetadata.GetOrAdd(key, request, (_, _, t) =>
            {
                var list = new List<NavigationCallback?>(2);
                if (t is IHasDisposeCallback hasDisposeCallback)
                    hasDisposeCallback.RegisterDisposeToken(GetDisposeTargetToken(t, list));
                return list;
            });
            lock (callbacks)
            {
                var callback = TryFindCallback(callbacks, callbackType, navigationId, navigationType, key);
                if (callback == null)
                {
                    callback = new NavigationCallback(callbackType, navigationId, navigationType);
                    callbacks.Add(callback);
                }

                return callback;
            }
        }

        public ItemOrIReadOnlyList<INavigationCallback> TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata)
        {
            var targetMetadata = GetTargetMetadata((request as IHasTarget<object?>)?.Target ?? request, true);
            if (targetMetadata.IsNullOrEmpty())
                return default;

            var list = new ItemOrListEditor<INavigationCallback>(3);
            AddCallbacks(InternalMetadata.ShowingCallbacks, targetMetadata, ref list);
            AddCallbacks(InternalMetadata.ClosingCallbacks, targetMetadata, ref list);
            AddCallbacks(InternalMetadata.CloseCallbacks, targetMetadata, ref list);
            return list;
        }

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext) =>
            InvokeCallbacks(navigationContext, callbackType, null, false, default);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            Exception exception) =>
            InvokeCallbacks(navigationContext, callbackType, exception, false, default);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            CancellationToken cancellationToken) =>
            InvokeCallbacks(navigationContext, callbackType, null, true, cancellationToken);

        private static ActionToken GetDisposeTargetToken(object target, List<NavigationCallback?> callbacks) =>
            ActionToken.FromDelegate((t, l) =>
            {
                var list = (List<NavigationCallback?>)l!;
                lock (list)
                {
                    if (list.Count == 0)
                        return;

                    for (var i = list.Count - 1; i >= 0; i--)
                    {
                        var navigationCallback = list[i];
                        if (navigationCallback == null)
                            continue;

                        var context = new NavigationContext(t, NavigationProvider.System, navigationCallback.NavigationId, navigationCallback.NavigationType, NavigationMode.Close);
                        navigationCallback.TrySetException(context, new ObjectDisposedException(t!.GetType().FullName));
                    }

                    list.Clear();
                }
            }, target, callbacks);

        private static NavigationCallback? TryFindCallback(List<NavigationCallback?> callbacks, NavigationCallbackType callbackType, string navigationId,
            NavigationType navigationType, IReadOnlyMetadataContextKey<List<NavigationCallback?>> key)
        {
            for (var i = 0; i < callbacks.Count; i++)
            {
                var callback = callbacks[i];
                if (callback != null && callback.NavigationId == navigationId && callback.CallbackType == callbackType && callback.NavigationType == navigationType)
                    return callback;
            }

            return null;
        }

        private static void AddCallbacks(IReadOnlyMetadataContextKey<List<NavigationCallback?>> key, IReadOnlyMetadataContext metadata,
            ref ItemOrListEditor<INavigationCallback> editor)
        {
            var callbacks = metadata.Get(key);
            if (callbacks == null)
                return;

            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                    editor.AddIfNotNull(callbacks[i]!);
            }
        }

        private static IMetadataContextKey<List<NavigationCallback?>>? GetKeyByCallback(NavigationCallbackType callbackType)
        {
            if (callbackType == NavigationCallbackType.Showing)
                return InternalMetadata.ShowingCallbacks;
            if (callbackType == NavigationCallbackType.Closing)
                return InternalMetadata.ClosingCallbacks;
            if (callbackType == NavigationCallbackType.Close)
                return InternalMetadata.CloseCallbacks;
            return null;
        }

        private bool InvokeCallbacks(INavigationContext navigationContext, NavigationCallbackType callbackType, Exception? exception, bool canceled,
            CancellationToken cancellationToken)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return false;

            var callbacks = GetTargetMetadata(navigationContext.Target, true)?.Get(key);
            if (callbacks == null)
                return false;

            var toInvoke = new ItemOrListEditor<NavigationCallback>(2);
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationId == navigationContext.NavigationId && callback.NavigationType == navigationContext.NavigationType)
                    {
                        toInvoke.AddIfNotNull(callback!);
                        callbacks.RemoveAt(i);
                        --i;
                    }
                }
            }

            if (toInvoke.IsEmpty)
                return false;

            foreach (var callback in toInvoke)
            {
                if (exception != null)
                    callback.SetException(navigationContext, exception);
                else if (canceled)
                    callback.SetCanceled(navigationContext, cancellationToken);
                else
                    callback.SetResult(navigationContext);
            }

            return true;
        }

        private IReadOnlyMetadataContext? GetTargetMetadata(object? target, bool isReadonly)
        {
            if (target == null)
                return null;

            if (target is IMetadataOwner<IReadOnlyMetadataContext> t)
            {
                if (isReadonly)
                    return t.GetMetadataOrDefault();
                if (t.Metadata is IMetadataContext ctx)
                    return ctx;
            }

            var attachedValues = target.AttachedValues(attachedValueManager: _attachedValueManager);
            if (isReadonly)
            {
                attachedValues.TryGet(InternalConstant.CallbackMetadataKey, out var m);
                return (IReadOnlyMetadataContext?)m;
            }

            return attachedValues.GetOrAdd(InternalConstant.CallbackMetadataKey, this, (_, _) => new MetadataContext());
        }
    }
}