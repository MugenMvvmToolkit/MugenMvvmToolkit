using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationCallbackManagerComponent : INavigationCallbackManagerComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationCallbackManagerComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<NavigationCallbackType, string, NavigationType, object, IReadOnlyMetadataContext?, INavigationCallback?>? TryAddNavigationCallback { get; set; }

        public Func<object, IReadOnlyMetadataContext?, ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>>>? TryGetNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, INavigationContext, bool>? TryInvokeNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, INavigationContext, Exception, bool>? TryInvokeExceptionNavigationCallbacks { get; set; }

        public Func<NavigationCallbackType, INavigationContext, CancellationToken, bool>? TryInvokeCanceledNavigationCallbacks { get; set; }

        #endregion

        #region Implementation of interfaces

        INavigationCallback? INavigationCallbackManagerComponent.TryAddNavigationCallback(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, string navigationId,
            NavigationType navigationType,
            object request, IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryAddNavigationCallback?.Invoke(callbackType, navigationId, navigationType, request, metadata);
        }

        ItemOrList<INavigationCallback, IReadOnlyList<INavigationCallback>> INavigationCallbackManagerComponent.TryGetNavigationCallbacks(INavigationDispatcher navigationDispatcher, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryGetNavigationCallbacks?.Invoke(request, metadata) ?? default;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryInvokeNavigationCallbacks?.Invoke(callbackType, navigationContext) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext, Exception exception)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryInvokeExceptionNavigationCallbacks?.Invoke(callbackType, navigationContext, exception) ?? false;
        }

        bool INavigationCallbackManagerComponent.TryInvokeNavigationCallbacks(INavigationDispatcher navigationDispatcher, NavigationCallbackType callbackType, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return TryInvokeCanceledNavigationCallbacks?.Invoke(callbackType, navigationContext, cancellationToken) ?? false;
        }

        #endregion
    }
}