using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationDispatcherErrorListener : INavigationDispatcherErrorListener
    {
        #region Properties

        public Action<INavigationDispatcher, INavigationContext, Exception>? OnNavigationFailed { get; set; }

        public Action<INavigationDispatcher, INavigationContext, CancellationToken>? OnNavigationCanceled { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherErrorListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            OnNavigationFailed?.Invoke(navigationDispatcher, navigationContext, exception);
        }

        void INavigationDispatcherErrorListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            OnNavigationCanceled?.Invoke(navigationDispatcher, navigationContext, cancellationToken);
        }

        #endregion
    }
}