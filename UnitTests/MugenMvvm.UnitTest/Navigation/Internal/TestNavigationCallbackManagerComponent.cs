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

        public Func<NavigationCallbackType, INavigationContext, bool>? TryInvokeNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, INavigationContext, Exception, bool>? TryInvokeExceptionNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, INavigationContext, CancellationToken, bool>? TryInvokeCanceledNavigationCallbacks { get; set; }

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

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext)
        {
            return TryInvokeNavigationCallbacks?.Invoke(callbackType, navigationContext) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception)
        {
            return TryInvokeExceptionNavigationCallbacks?.Invoke(callbackType, navigationContext, exception) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            return TryInvokeCanceledNavigationCallbacks?.Invoke(callbackType, navigationContext, cancellationToken) ?? false;
        }

        #endregion
    }
}