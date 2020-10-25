using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationListener : INavigationListener, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Action<INavigationContext>? OnNavigating { get; set; }

        public Action<INavigationContext>? OnNavigated { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationListener.OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigating?.Invoke(navigationContext);
        }

        void INavigationListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigated?.Invoke(navigationContext);
        }

        #endregion
    }
}