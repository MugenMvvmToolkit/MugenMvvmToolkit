using System;
using System.Threading;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationDispatcherErrorListener : INavigationDispatcherErrorListener
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationDispatcherErrorListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Action<INavigationContext, Exception>? OnNavigationFailed { get; set; }

        public Action<INavigationContext, CancellationToken>? OnNavigationCanceled { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherErrorListener.OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationFailed?.Invoke(navigationContext, exception);
        }

        void INavigationDispatcherErrorListener.OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationCanceled?.Invoke(navigationContext, cancellationToken);
        }

        #endregion
    }
}