using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationDispatcherNavigatedListener : INavigationDispatcherNavigatedListener, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationDispatcherNavigatedListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Action<INavigationContext>? OnNavigated { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherNavigatedListener.OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigated?.Invoke(navigationContext);
        }

        #endregion
    }
}