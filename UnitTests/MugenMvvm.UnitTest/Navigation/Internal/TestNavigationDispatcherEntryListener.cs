using System;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationDispatcherEntryListener : INavigationDispatcherEntryListener
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationDispatcherEntryListener(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Action<INavigationEntry, INavigationContext?>? OnNavigationEntryAdded { get; set; }

        public Action<INavigationEntry, INavigationContext?>? OnNavigationEntryUpdated { get; set; }

        public Action<INavigationEntry, INavigationContext?>? OnNavigationEntryRemoved { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherEntryListener.OnNavigationEntryAdded(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationEntryAdded?.Invoke(navigationEntry, navigationContext);
        }

        void INavigationDispatcherEntryListener.OnNavigationEntryUpdated(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationEntryUpdated?.Invoke(navigationEntry, navigationContext);
        }

        void INavigationDispatcherEntryListener.OnNavigationEntryRemoved(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            OnNavigationEntryRemoved?.Invoke(navigationEntry, navigationContext);
        }

        #endregion
    }
}