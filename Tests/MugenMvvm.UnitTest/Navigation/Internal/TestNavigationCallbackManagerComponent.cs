using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationCallbackManagerComponent : INavigationCallbackManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<NavigationCallbackType, object, Type, IReadOnlyMetadataContext?, INavigationCallback?>? TryAddNavigationCallback { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>>>? TryGetNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, object, Type, IReadOnlyMetadataContext?, bool>? TryInvokeNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, object, Type, Exception, IReadOnlyMetadataContext?, bool>? TryInvokeExceptionNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, object, Type, CancellationToken, IReadOnlyMetadataContext?, bool>? TryInvokeCanceledNavigationCallbacks { get; set; }

        #endregion

        #region Implementation of interfaces

        INavigationCallback? INavigationCallbackManagerComponent.TryAddNavigationCallback<TRequest>(NavigationCallbackType callbackType, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryAddNavigationCallback?.Invoke(callbackType, request!, typeof(TRequest), metadata);
        }

        ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> INavigationCallbackManagerComponent.TryGetNavigationCallbacks<TTarget>(in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return TryGetNavigationCallbacks?.Invoke(target!, typeof(TTarget), metadata) ?? default;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return TryInvokeNavigationCallbacks?.Invoke(callbackType, target!, typeof(TTarget), metadata) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, in TTarget target, Exception exception, IReadOnlyMetadataContext? metadata)
        {
            return TryInvokeExceptionNavigationCallbacks?.Invoke(callbackType, target!, typeof(TTarget), exception, metadata) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks<TTarget>(NavigationCallbackType callbackType, in TTarget target, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return TryInvokeCanceledNavigationCallbacks?.Invoke(callbackType, target!, typeof(TTarget), cancellationToken, metadata) ?? false;
        }

        #endregion
    }
}