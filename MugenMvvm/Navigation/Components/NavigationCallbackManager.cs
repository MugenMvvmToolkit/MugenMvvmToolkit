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

        private static void AddCallback(IMetadataContextKey<List<NavigationCallback?>> key, NavigationCallback callback, IMetadataContext? metadata)
        {
            var callbacks = metadata?.GetOrAdd(key, key, (_, __, ___) => new List<NavigationCallback?>());
            if (callbacks == null)
                return;
            lock (callbacks)
            {
                callbacks.Add(callback);
            }
        }

        private static NavigationCallback? TryFindCallback(NavigationCallbackType callbackType, string navigationId, NavigationType navigationType,
            IReadOnlyMetadataContextKey<List<NavigationCallback?>> key,
            IReadOnlyMetadataContext metadata)
        {
            var callbacks = metadata.Get(key);
            if (callbacks == null)
                return null;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback != null && callback.NavigationId == navigationId && callback.CallbackType == callbackType && callback.NavigationType == navigationType)
                        return callback;
                }
            }

            return null;
        }

        private static void AddCallbacks(IReadOnlyMetadataContextKey<List<NavigationCallback?>> key, IReadOnlyMetadataContext? metadata,
            ref ItemOrListEditor<INavigationCallback> editor)
        {
            var callbacks = metadata?.Get(key);
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

        public INavigationCallback? TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId,
            NavigationType navigationType, object request,
            IReadOnlyMetadataContext? metadata)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return null;

            var targetMetadata = (IMetadataContext?) GetTargetMetadata((request as IHasTarget<object?>)?.Target ?? request, false);
            if (targetMetadata == null)
                return null;

            var callback = TryFindCallback(callbackType, navigationId, navigationType, key, targetMetadata);
            if (callback == null)
            {
                callback = new NavigationCallback(callbackType, navigationId, navigationType);
                AddCallback(key, callback, targetMetadata);
            }

            if (request is IMetadataOwner<IReadOnlyMetadataContext> owner && owner.Metadata is IMetadataContext m && m != targetMetadata)
                AddCallback(key, callback, m);
            return callback;
        }

        public ItemOrIReadOnlyList<INavigationCallback> TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata) =>
            GetCallbacks((request as IMetadataOwner<IReadOnlyMetadataContext>)?.GetMetadataOrDefault(), (request as IHasTarget<object?>)?.Target ?? request);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext) =>
            InvokeCallbacks(navigationContext, callbackType, null, false, default);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            Exception exception) =>
            InvokeCallbacks(navigationContext, callbackType, exception, false, default);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            CancellationToken cancellationToken) =>
            InvokeCallbacks(navigationContext, callbackType, null, true, cancellationToken);

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

            for (var i = 0; i < toInvoke.Count; i++)
            {
                var callback = toInvoke[i];
                if (exception != null)
                    callback.SetException(navigationContext, exception);
                else if (canceled)
                    callback.SetCanceled(navigationContext, cancellationToken);
                else
                    callback.SetResult(navigationContext);
            }

            return true;
        }

        private ItemOrIReadOnlyList<INavigationCallback> GetCallbacks(IReadOnlyMetadataContext? metadata, object target)
        {
            var canMoveNext = true;
            var list = new ItemOrListEditor<INavigationCallback>(3);
            while (true)
            {
                AddCallbacks(InternalMetadata.ShowingCallbacks, metadata, ref list);
                AddCallbacks(InternalMetadata.ClosingCallbacks, metadata, ref list);
                AddCallbacks(InternalMetadata.CloseCallbacks, metadata, ref list);

                if (!list.IsEmpty || !canMoveNext)
                    break;
                if (list.IsEmpty)
                    metadata = GetTargetMetadata(target, true);
                canMoveNext = false;
            }

            return list.ToItemOrList();
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
                return (IReadOnlyMetadataContext?) m;
            }

            return attachedValues.GetOrAdd(InternalConstant.CallbackMetadataKey, this, (o, manager) => new MetadataContext());
        }
    }
}