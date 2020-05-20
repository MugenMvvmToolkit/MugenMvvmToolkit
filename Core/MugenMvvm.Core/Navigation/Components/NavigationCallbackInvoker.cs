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
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.CallbackInvoker;

        #endregion

        #region Methods

        protected override void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            InvokeCallbacks(navigationContext, NavigationCallbackType.Closing, exception, false, default);
            InvokeCallbacks(navigationContext, NavigationCallbackType.Showing, exception, false, default);
            InvokeCallbacks(navigationContext, NavigationCallbackType.Close, exception, false, default);
        }

        protected override void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.NavigationMode.IsClose)
                InvokeCallbacks(navigationContext, NavigationCallbackType.Closing, null, true, cancellationToken);
            else
            {
                InvokeCallbacks(navigationContext, NavigationCallbackType.Showing, null, true, cancellationToken);
                if (navigationContext.NavigationMode.IsNew)
                {
                    InvokeCallbacks(navigationContext, NavigationCallbackType.Closing, null, true, cancellationToken);
                    InvokeCallbacks(navigationContext, NavigationCallbackType.Close, null, true, cancellationToken);
                }
            }
        }

        protected override void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode.IsClose)
            {
                InvokeCallbacks(navigationContext, NavigationCallbackType.Showing, null, false, default);
                InvokeCallbacks(navigationContext, NavigationCallbackType.Closing, null, false, default);
                InvokeCallbacks(navigationContext, NavigationCallbackType.Close, null, false, default);
            }
            else
                InvokeCallbacks(navigationContext, NavigationCallbackType.Showing, null, false, default);
        }

        private void InvokeCallbacks(INavigationContext navigationContext, NavigationCallbackType callbackType, Exception? exception, bool canceled, CancellationToken cancellationToken)
        {
            var metadata = navigationContext.GetMetadataOrDefault();
            var components = Owner.GetComponents<INavigationCallbackManagerComponent>(metadata);
            if (exception != null)
                components.TryInvokeNavigationCallbacks(callbackType, navigationContext, exception, metadata);
            else if (canceled)
                components.TryInvokeNavigationCallbacks(callbackType, navigationContext, cancellationToken, metadata);
            else
                components.TryInvokeNavigationCallbacks(callbackType, navigationContext, metadata);
        }

        #endregion
    }
}