using System;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationCallbackManagerComponent : INavigationCallbackManagerComponent, IHasPriority
    {
        public Func<INavigationDispatcher, NavigationCallbackType, string, NavigationType, object, IReadOnlyMetadataContext?, INavigationCallback?>? TryAddNavigationCallback
        {
            get;
            set;
        }

        public Func<INavigationDispatcher, object, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<INavigationCallback>>? TryGetNavigationCallbacks { get; set; }

        public Func<INavigationDispatcher, NavigationCallbackType, INavigationContext, bool>? TryInvokeNavigationCallbacks { get; set; }

        public Func<INavigationDispatcher, NavigationCallbackType, INavigationContext, Exception, bool>? TryInvokeExceptionNavigationCallbacks { get; set; }

        public Func<INavigationDispatcher, NavigationCallbackType, INavigationContext, CancellationToken, bool>? TryInvokeCanceledNavigationCallbacks { get; set; }

        public int Priority { get; set; }

        INavigationCallback? INavigationCallbackManagerComponent.TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType,
            string navigationId, NavigationType navigationType, object request, IReadOnlyMetadataContext? metadata) =>
            TryAddNavigationCallback?.Invoke(navigationDispatcher, callbackType, navigationId, navigationType, request, metadata);

        ItemOrIReadOnlyList<INavigationCallback> INavigationCallbackManagerComponent.TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request,
            IReadOnlyMetadataContext? metadata) =>
            TryGetNavigationCallbacks?.Invoke(navigationDispatcher, request, metadata) ?? default;

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType,
            INavigationContext navigationContext) =>
            TryInvokeNavigationCallbacks?.Invoke(navigationDispatcher, callbackType, navigationContext) ?? false;

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType,
            INavigationContext navigationContext, Exception exception) =>
            TryInvokeExceptionNavigationCallbacks?.Invoke(navigationDispatcher, callbackType, navigationContext, exception) ?? false;

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType,
            INavigationContext navigationContext,
            CancellationToken cancellationToken) =>
            TryInvokeCanceledNavigationCallbacks?.Invoke(navigationDispatcher, callbackType, navigationContext, cancellationToken) ?? false;
    }
}