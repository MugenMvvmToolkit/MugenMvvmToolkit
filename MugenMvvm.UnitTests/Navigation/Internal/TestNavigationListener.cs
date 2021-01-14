using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationListener : INavigationListener, IHasPriority
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        public TestNavigationListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public Action<INavigationContext>? OnNavigating { get; set; }

        public Action<INavigationContext>? OnNavigated { get; set; }

        public int Priority { get; set; }

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
    }
}