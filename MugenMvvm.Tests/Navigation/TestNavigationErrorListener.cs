using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationErrorListener : INavigationErrorListener
    {
        public Action<INavigationDispatcher, INavigationContext, Exception>? OnNavigationFailed { get; set; }

        public Action<INavigationDispatcher, INavigationContext, CancellationToken>? OnNavigationCanceled { get; set; }

        void INavigationErrorListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception) =>
            OnNavigationFailed?.Invoke(navigationDispatcher, navigationContext, exception);

        void INavigationErrorListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken) =>
            OnNavigationCanceled?.Invoke(navigationDispatcher, navigationContext, cancellationToken);
    }
}