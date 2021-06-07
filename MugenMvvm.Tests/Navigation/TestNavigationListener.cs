using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationListener : INavigationListener, IHasPriority
    {
        public Action<INavigationDispatcher, INavigationContext>? OnNavigating { get; set; }

        public Action<INavigationDispatcher, INavigationContext>? OnNavigated { get; set; }

        public int Priority { get; set; }

        void INavigationListener.OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) =>
            OnNavigating?.Invoke(navigationDispatcher, navigationContext);

        void INavigationListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) =>
            OnNavigated?.Invoke(navigationDispatcher, navigationContext);
    }
}