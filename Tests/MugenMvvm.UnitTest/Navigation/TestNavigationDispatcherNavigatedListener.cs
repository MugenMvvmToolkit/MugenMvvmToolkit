using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation
{
    public class TestNavigationDispatcherNavigatedListener : INavigationDispatcherNavigatedListener, IHasPriority
    {
        #region Properties

        public Action<INavigationDispatcher, INavigationContext>? OnNavigated { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherNavigatedListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            OnNavigated?.Invoke(navigationDispatcher, navigationContext);
        }

        #endregion
    }
}