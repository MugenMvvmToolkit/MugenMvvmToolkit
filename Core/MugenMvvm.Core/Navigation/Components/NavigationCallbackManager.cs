using System;
using System.Collections.Generic;
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

        public INavigationCallback? TryAddNavigationCallback<TTarget>(NavigationCallbackType callbackType, in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return null;
            var callback = TryGetNavigationCallback(callbackType, target, out var targetMetadata, out var contextMetadata);
            if (callback != null)
            {
                AddCallback(key, callback, targetMetadata);
                AddCallback(key, callback, contextMetadata);
            }

            return callback;
        }

        public IReadOnlyList<INavigationCallback>? TryGetNavigationCallbacks<TTarget>(in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            if (!Default.IsValueType<TTarget>() && target is IMetadataOwner<IReadOnlyMetadataContext> owner)
                return GetCallbacks(owner.GetMetadataOrDefault(), target as IHasTarget);
            return null;
        }

        public bool TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return InvokeCallbacks(target, callbackType, null, false, default);
        }

        public bool TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, in TTarget target, Exception exception, IReadOnlyMetadataContext? metadata)
        {
            return InvokeCallbacks(target, callbackType, exception, false, default);
        }

        public bool TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, in TTarget target, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return InvokeCallbacks(target, callbackType, null, true, cancellationToken);
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

            if (!(context is IHasNavigationInfo hasNavigationInfo))
                return false;

            var metadata = (context as IMetadataOwner<IReadOnlyMetadataContext>)?.GetMetadataOrDefault();
            var target = metadata?.Get(NavigationMetadata.Target) as IMetadataOwner<IMetadataContext>;
            var callbacks = target?.GetMetadataOrDefault().Get(key);
            if (callbacks == null)
                return false;

            ItemOrList<NavigationCallback, List<NavigationCallback>> toInvoke = default;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationId == hasNavigationInfo.NavigationId)
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
            lock (callback)
            {
                callbacks.Add(callback);
            }
        }

        private static NavigationCallback? TryGetNavigationCallback<TTarget>(NavigationCallbackType callbackType, in TTarget request, out IMetadataContext? targetMetadata, out IMetadataContext? contextMetadata)
        {
            targetMetadata = null;
            contextMetadata = null;
            if (Default.IsValueType<TTarget>() || !(request is IHasNavigationInfo hasNavigationInfo))
                return null;

            contextMetadata = (request as IMetadataOwner<IReadOnlyMetadataContext>)?.Metadata as IMetadataContext;
            if (request is IHasTarget hasTarget && hasTarget.Target is IMetadataOwner<IMetadataContext> targetOwner)
            {
                targetMetadata = targetOwner.Metadata;
                return new NavigationCallback(callbackType, hasNavigationInfo.NavigationId, hasNavigationInfo.NavigationType);
            }

            if (contextMetadata?.Get(NavigationMetadata.Target) is IMetadataOwner<IMetadataContext> owner)
            {
                targetMetadata = owner.Metadata;
                return new NavigationCallback(callbackType, hasNavigationInfo.NavigationId, hasNavigationInfo.NavigationType);
            }

            return null;
        }

        private static IMetadataContextKey<List<NavigationCallback>, List<NavigationCallback>> GetKey(string name)
        {
            return MetadataContextKey.Create<List<NavigationCallback>, List<NavigationCallback>>(typeof(NavigationCallbackManager), name)
                .Serializable()
                .Build();
        }

        private static IReadOnlyList<NavigationCallback>? GetCallbacks(IReadOnlyMetadataContext metadata, IHasTarget? hasTarget)
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