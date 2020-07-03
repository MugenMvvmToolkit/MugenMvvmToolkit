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

        public Func<INavigationDispatcher, NavigationCallbackType, object, Type, IReadOnlyMetadataContext?, INavigationCallback?>? TryAddNavigationCallback { get; set; }

        public Func<INavigationDispatcher, object, Type, IReadOnlyMetadataContext?, ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>>>? TryGetNavigationCallbacks { get; set; }

        public Func<INavigationDispatcher, NavigationCallbackType, INavigationContext, bool>? TryInvokeNavigationCallbacks { get; set; }

        public Func<INavigationDispatcher, NavigationCallbackType, INavigationContext, Exception, bool>? TryInvokeExceptionNavigationCallbacks { get; set; }

        public Func<INavigationDispatcher, NavigationCallbackType, INavigationContext, CancellationToken, bool>? TryInvokeCanceledNavigationCallbacks { get; set; }

        #endregion

        #region Implementation of interfaces

        INavigationCallback? INavigationCallbackManagerComponent.TryAddNavigationCallback<TRequest>(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryAddNavigationCallback?.Invoke(navigationDispatcher, callbackType, request!, typeof(TRequest), metadata);
        }

        ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> INavigationCallbackManagerComponent.TryGetNavigationCallbacks<TTarget>(INavigationDispatcher navigationDispatcher, in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return TryGetNavigationCallbacks?.Invoke(navigationDispatcher, target!, typeof(TTarget), metadata) ?? default;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext)
        {
            return TryInvokeNavigationCallbacks?.Invoke(navigationDispatcher, callbackType, navigationContext) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception)
        {
            return TryInvokeExceptionNavigationCallbacks?.Invoke(navigationDispatcher, callbackType, navigationContext, exception) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            return TryInvokeCanceledNavigationCallbacks?.Invoke(navigationDispatcher, callbackType, navigationContext, cancellationToken) ?? false;
        }

        #endregion
    }
}