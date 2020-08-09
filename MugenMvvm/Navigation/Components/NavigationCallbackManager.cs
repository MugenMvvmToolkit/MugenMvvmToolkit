using System;
using System.Collections.Generic;
using System.Threading;
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
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;

        private static readonly IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> ShowingCallbacks = GetKey(nameof(ShowingCallbacks));
        private static readonly IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> ClosingCallbacks = GetKey(nameof(ClosingCallbacks));
        private static readonly IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> CloseCallbacks = GetKey(nameof(CloseCallbacks));

        #endregion

        #region Constructors

        public NavigationCallbackManager(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.CallbackManager;

        #endregion

        #region Implementation of interfaces

        public INavigationCallback? TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId, NavigationType navigationType, object request,
            IReadOnlyMetadataContext? metadata)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return null;

            var targetMetadata = (IMetadataContext?) GetTargetMetadata((request as IHasTarget<object?>)?.Target ?? request, false);
            if (targetMetadata != null)
            {
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

            return null;
        }

        public ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request, IReadOnlyMetadataContext? metadata) =>
            GetCallbacks((request as IMetadataOwner<IReadOnlyMetadataContext>)?.GetMetadataOrDefault(), (request as IHasTarget<object?>)?.Target ?? request);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext) =>
            InvokeCallbacks(navigationContext, callbackType, null, false, default);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception) =>
            InvokeCallbacks(navigationContext, callbackType, exception, false, default);

        public bool TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken) =>
            InvokeCallbacks(navigationContext, callbackType, null, true, cancellationToken);

        #endregion

        #region Methods

        private bool InvokeCallbacks(INavigationContext navigationContext, NavigationCallbackType callbackType, Exception? exception, bool canceled, CancellationToken cancellationToken)
        {
            var key = GetKeyByCallback(callbackType);
            if (key == null)
                return false;

            var callbacks = GetTargetMetadata(navigationContext.Target, true)?.Get(key);
            if (callbacks == null)
                return false;

            var toInvoke = ItemOrListEditor.Get<NavigationCallback>();
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

            if (toInvoke.IsNullOrEmpty)
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

        private static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> GetKey(string name) =>
            MetadataContextKey.Create<List<NavigationCallback?>, List<NavigationCallback?>>(typeof(NavigationCallbackManager), name)
                .Serializable()
                .Build();

        private ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> GetCallbacks(IReadOnlyMetadataContext? metadata, object target)
        {
            var canMoveNext = true;
            var list = ItemOrListEditor.Get<INavigationCallback>();
            while (true)
            {
                AddCallbacks(ShowingCallbacks, metadata, ref list);
                AddCallbacks(ClosingCallbacks, metadata, ref list);
                AddCallbacks(CloseCallbacks, metadata, ref list);

                if (!list.IsNullOrEmpty || !canMoveNext)
                    break;
                if (list.IsNullOrEmpty)
                    metadata = GetTargetMetadata(target, true);
                canMoveNext = false;
            }

            return list.ToItemOrList<IReadOnlyList<INavigationCallback>>();
        }

        private static void AddCallbacks(IReadOnlyMetadataContextKey<List<NavigationCallback?>> key, IReadOnlyMetadataContext? metadata, ref ItemOrListEditor<INavigationCallback, List<INavigationCallback>> list)
        {
            var callbacks = metadata?.Get(key);
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

            var attachedValues = _attachedValueManager.DefaultIfNull().TryGetAttachedValues(target);
            if (isReadonly)
            {
                attachedValues.TryGet(InternalConstant.CallbackMetadataKey, out var m);
                return (IReadOnlyMetadataContext?) m;
            }

            return attachedValues.GetOrAdd(InternalConstant.CallbackMetadataKey, this, (o, manager) => new MetadataContext());
        }

        #endregion
    }
}