using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
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
        #region Fields

        private static readonly IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>> ShowingCallbacks = GetKey(nameof(ShowingCallbacks));
        private static readonly IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>> ClosingCallbacks = GetKey(nameof(ClosingCallbacks));
        private static readonly IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>> CloseCallbacks = GetKey(nameof(CloseCallbacks));

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.CallbackManager;

        #endregion

        #region Implementation of interfaces

        public INavigationCallback? TryAddNavigationCallback<TRequest>(NavigationCallbackType callbackType, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return null;
            if (TryGetCallbackTarget(request, out var target, out var contextMetadata, out var navigationId, out var navigationType)
                && target.Metadata is IMetadataContext targetMetadata)
            {
                var callback = TryFindCallback(callbackType, navigationId, navigationType, key, targetMetadata);
                if (callback == null)
                {
                    callback = new NavigationCallback(callbackType, navigationId, navigationType);
                    AddCallback(key, callback, targetMetadata);
                }

                AddCallback(key, callback, contextMetadata as IMetadataContext);
                return callback;
            }

            return null;
        }

        public IReadOnlyList<INavigationCallback>? TryGetNavigationCallbacks<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (!Default.IsValueType<TRequest>() && request is IMetadataOwner<IReadOnlyMetadataContext> owner)
                return GetCallbacks(owner.GetMetadataOrDefault(), request as IHasTarget<object?>);
            return null;
        }

        public bool TryInvokeNavigationCallbacks<TRequest>(NavigationCallbackType callbackType, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return InvokeCallbacks(request, callbackType, null, false, default);
        }

        public bool TryInvokeNavigationCallbacks<TRequest>(NavigationCallbackType callbackType, in TRequest request, Exception exception, IReadOnlyMetadataContext? metadata)
        {
            return InvokeCallbacks(request, callbackType, exception, false, default);
        }

        public bool TryInvokeNavigationCallbacks<TRequest>(NavigationCallbackType callbackType, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return InvokeCallbacks(request, callbackType, null, true, cancellationToken);
        }

        #endregion

        #region Methods

        private static bool InvokeCallbacks<T>(in T context, NavigationCallbackType callbackType, Exception? exception, bool canceled, CancellationToken cancellationToken)
        {
            if (Default.IsValueType<T>())
                return false;

            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return false;

            if (!TryGetCallbackTarget(context, out var target, out var metadata, out var navigationId, out var navigationType))
                return false;

            var callbacks = target.GetMetadataOrDefault().Get(key);
            if (callbacks == null)
                return false;

            ItemOrList<NavigationCallback, List<NavigationCallback>> toInvoke = default;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationId == navigationId && callback.NavigationType == navigationType)
                    {
                        if (callback != null)
                            toInvoke.Add(callback);
                        callbacks.RemoveAt(i);
                        --i;
                    }
                }
            }

            if (toInvoke.IsNullOrEmpty())
                return false;

            for (var i = 0; i < toInvoke.Count(); i++)
            {
                var callback = toInvoke.Get(i);
                if (exception != null)
                    callback.SetException(exception, metadata!);
                else if (canceled)
                    callback.SetCanceled(metadata!, cancellationToken);
                else
                    callback.SetResult(metadata!);
            }

            return true;
        }

        private static void AddCallback(IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>> key, NavigationCallback callback, IMetadataContext? metadata)
        {
            var callbacks = metadata?.GetOrAdd(key, key, (context, _) => new List<NavigationCallback>());
            if (callbacks == null)
                return;
            lock (callbacks)
            {
                callbacks.Add(callback);
            }
        }

        private static NavigationCallback? TryFindCallback(NavigationCallbackType callbackType, string navigationId, NavigationType navigationType, IReadOnlyMetadataContextKey<List<NavigationCallback>> key, IReadOnlyMetadataContext metadata)
        {
            var callbacks = metadata?.Get(key);
            if (callbacks == null)
                return null;
            lock (callbacks)
            {
                for (int i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback != null && callback.NavigationId == navigationId && callback.CallbackType == callbackType && callback.NavigationType == navigationType)
                        return callback;
                }
            }

            return null;
        }

        private static bool TryGetCallbackTarget<TRequest>(in TRequest request, [NotNullWhen(true)] out IMetadataOwner<IReadOnlyMetadataContext>? target, out IReadOnlyMetadataContext? contextMetadata,
            [NotNullWhen(true)] out string? navigationId, [NotNullWhen(true)]out NavigationType? navigationType)
        {
            target = null;
            contextMetadata = null;
            navigationId = null;
            navigationType = null;
            if (Default.IsValueType<TRequest>() || !(request is IHasNavigationInfo hasNavigationInfo))
                return false;

            navigationId = hasNavigationInfo.NavigationId;
            navigationType = hasNavigationInfo.NavigationType;
            contextMetadata = (request as IMetadataOwner<IReadOnlyMetadataContext>)?.GetMetadataOrDefault();
            if (request is IHasTarget<object?> hasTarget && hasTarget.Target is IMetadataOwner<IReadOnlyMetadataContext> targetOwner)
            {
                target = targetOwner;
                return true;
            }

            if (contextMetadata?.Get(NavigationMetadata.Target) is IMetadataOwner<IReadOnlyMetadataContext> owner)
            {
                target = owner;
                return true;
            }

            return false;
        }

        private static IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>> GetKey(string name)
        {
            return MetadataContextKey.Create<List<NavigationCallback>, List<NavigationCallback>>(typeof(NavigationCallbackManager), name)
                .Serializable()
                .Build();
        }

        private static IReadOnlyList<NavigationCallback>? GetCallbacks(IReadOnlyMetadataContext metadata, IHasTarget<object?>? hasTarget)
        {
            var canMoveNext = true;
            LazyList<NavigationCallback> list = default;
            while (true)
            {
                AddCallbacks(ShowingCallbacks, metadata, ref list);
                AddCallbacks(ClosingCallbacks, metadata, ref list);
                AddCallbacks(CloseCallbacks, metadata, ref list);

                if (list.List != null || !canMoveNext)
                    break;
                if (list.List == null && hasTarget != null && hasTarget.Target is IMetadataOwner<IReadOnlyMetadataContext> targetOwner)
                    metadata = targetOwner.GetMetadataOrDefault();
                else if (list.List == null && metadata.Get(NavigationMetadata.Target) is IMetadataOwner<IReadOnlyMetadataContext> owner)
                    metadata = owner.GetMetadataOrDefault();
                canMoveNext = false;
            }

            return list.List;
        }

        private static void AddCallbacks(IReadOnlyMetadataContextKey<List<NavigationCallback>> key, IReadOnlyMetadataContext metadata, ref LazyList<NavigationCallback> list)
        {
            var callbacks = metadata.Get(key);
            if (callbacks == null)
                return;

            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback != null)
                        list.Add(callback);
                }
            }
        }

        private static IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>>? GetKeyByCallback(NavigationCallbackType callbackType)
        {
            if (callbackType == NavigationCallbackType.Showing)
                return ShowingCallbacks;
            if (callbackType == NavigationCallbackType.Closing)
                return ClosingCallbacks;
            if (callbackType == NavigationCallbackType.Close)
                return CloseCallbacks;
            return null;
        }

        #endregion
    }
}