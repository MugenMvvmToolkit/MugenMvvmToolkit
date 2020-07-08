using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationDispatcherNavigatingListener : INavigationDispatcherNavigatingListener, IHasPriority
    {
        #region Properties

        public Action<INavigationDispatcher, INavigationContext>? OnNavigating { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherNavigatingListener.OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            OnNavigating?.Invoke(navigationDispatcher, navigationContext);
        }

        #endregion
    }
}