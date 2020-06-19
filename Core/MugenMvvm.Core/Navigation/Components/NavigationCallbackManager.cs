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

        private static readonly IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> ShowingCallbacks = GetKey(nameof(ShowingCallbacks));
        private static readonly IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> ClosingCallbacks = GetKey(nameof(ClosingCallbacks));
        private static readonly IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> CloseCallbacks = GetKey(nameof(CloseCallbacks));

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.CallbackManager;

        #endregion

        #region Implementation of interfaces

        public INavigationCallback? TryAddNavigationCallback<TRequest>(NavigationCallbackType callbackType, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is IHasNavigationInfo hasNavigationInfo))
                return null;

            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return null;

            if (request is IHasTarget<object?> hasTarget && hasTarget.Target is IMetadataOwner<IReadOnlyMetadataContext> targetOwner && targetOwner.Metadata is IMetadataContext targetMetadata)
            {
                var callback = TryFindCallback(callbackType, hasNavigationInfo.NavigationId, hasNavigationInfo.NavigationType, key, targetMetadata);
                if (callback == null)
                {
                    callback = new NavigationCallback(callbackType, hasNavigationInfo.NavigationId, hasNavigationInfo.NavigationType);
                    AddCallback(key, callback, targetMetadata);
                }

                AddCallback(key, callback, (request as IMetadataOwner<IReadOnlyMetadataContext>)?.Metadata as IMetadataContext);
                return callback;
            }

            return null;
        }

        public ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> TryGetNavigationCallbacks<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (!TypeChecker.IsValueType<TRequest>() && request is IMetadataOwner<IReadOnlyMetadataContext> owner)
                return GetCallbacks(owner.GetMetadataOrDefault(), request as IHasTarget<object?>);
            return default;
        }

        public bool TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext)
        {
            return InvokeCallbacks(navigationContext, callbackType, null, false, default);
        }

        public bool TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception)
        {
            return InvokeCallbacks(navigationContext, callbackType, exception, false, default);
        }

        public bool TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            return InvokeCallbacks(navigationContext, callbackType, null, true, cancellationToken);
        }

        #endregion

        #region Methods

        private static bool InvokeCallbacks(INavigationContext navigationContext, NavigationCallbackType callbackType, Exception? exception, bool canceled, CancellationToken cancellationToken)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return false;

            var callbacks = (navigationContext.Target as IMetadataOwner<IReadOnlyMetadataContext>)?.GetMetadataOrDefault().Get(key);
            if (callbacks == null)
                return false;

            ItemOrList<NavigationCallback, List<NavigationCallback>> toInvoke = default;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationId == navigationContext.NavigationId && callback.NavigationType == navigationContext.NavigationType)
                    {
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
                    callback.SetException(navigationContext, exception);
                else if (canceled)
                    callback.SetCanceled(navigationContext, cancellationToken);
                else
                    callback.SetResult(navigationContext);
            }

            return true;
        }

        private static void AddCallback(IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> key, NavigationCallback callback, IMetadataContext? metadata)
        {
            var callbacks = metadata?.GetOrAdd(key, key, (context, _) => new List<NavigationCallback?>());
            if (callbacks == null)
                return;
            lock (callbacks)
            {
                callbacks.Add(callback);
            }
        }

        private static NavigationCallback? TryFindCallback(NavigationCallbackType callbackType, string navigationId, NavigationType navigationType, IReadOnlyMetadataContextKey<List<NavigationCallback?>> key,
            IReadOnlyMetadataContext metadata)
        {
            var callbacks = metadata?.Get(key);
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

        private static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> GetKey(string name)
        {
            return MetadataContextKey.Create<List<NavigationCallback?>, List<NavigationCallback?>>(typeof(NavigationCallbackManager), name)
                .Serializable()
                .Build();
        }

        private static ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> GetCallbacks(IReadOnlyMetadataContext metadata, IHasTarget<object?>? hasTarget)
        {
            var canMoveNext = true;
            ItemOrList<INavigationCallback, List<INavigationCallback>> list = default;
            while (true)
            {
                AddCallbacks(ShowingCallbacks, metadata, ref list);
                AddCallbacks(ClosingCallbacks, metadata, ref list);
                AddCallbacks(CloseCallbacks, metadata, ref list);

                if (!list.IsNullOrEmpty() || !canMoveNext)
                    break;
                if (list.IsNullOrEmpty() && hasTarget != null && hasTarget.Target is IMetadataOwner<IReadOnlyMetadataContext> targetOwner)
                    metadata = targetOwner.GetMetadataOrDefault();
                canMoveNext = false;
            }

            return list.Cast<IReadOnlyList<INavigationCallback>>();
        }

        private static void AddCallbacks(IReadOnlyMetadataContextKey<List<NavigationCallback?>> key, IReadOnlyMetadataContext metadata, ref ItemOrList<INavigationCallback, List<INavigationCallback>> list)
        {
            var callbacks = metadata.Get(key);
            if (callbacks == null)
                return;

            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                    list.Add(callbacks[i]);
            }
        }

        private static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>>? GetKeyByCallback(NavigationCallbackType callbackType)
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