using System;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationCallbackInvoker : SuspendableNavigationListenerBase, IHasPriority
    {
        public int Priority { get; init; } = NavigationComponentPriority.CallbackInvoker;

        protected override void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Closing, exception, false, default);
            InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Showing, exception, false, default);
            InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Close, exception, false, default);
        }

        protected override void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.NavigationMode.IsClose)
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Closing, null, true, cancellationToken);
            else
            {
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Showing, null, true, cancellationToken);
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Closing, null, true, cancellationToken);
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Close, null, true, cancellationToken);
            }
        }

        protected override void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
        }

        protected override void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Showing, null, false, default);
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Closing, null, false, default);
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Close, null, false, default);
            }
            else
                InvokeCallbacks(navigationDispatcher, navigationContext, NavigationCallbackType.Showing, null, false, default);
        }

        private void InvokeCallbacks(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, NavigationCallbackType callbackType, Exception? exception,
            bool canceled, CancellationToken cancellationToken)
        {
            var components = Owner.GetComponents<INavigationCallbackManagerComponent>(navigationContext.GetMetadataOrDefault());
            if (exception != null)
                components.TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext, exception);
            else if (canceled)
                components.TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext, cancellationToken);
            else
                components.TryInvokeNavigationCallbacks(navigationDispatcher, callbackType, navigationContext);
        }
    }
}