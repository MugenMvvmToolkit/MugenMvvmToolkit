using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationErrorListener : INavigationErrorListener
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        public TestNavigationErrorListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public Action<INavigationContext, Exception>? OnNavigationFailed { get; set; }

        public Action<INavigationContext, CancellationToken>? OnNavigationCanceled { get; set; }

        void INavigationErrorListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationFailed?.Invoke(navigationContext, exception);
        }

        void INavigationErrorListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationCanceled?.Invoke(navigationContext, cancellationToken);
        }
    }
}