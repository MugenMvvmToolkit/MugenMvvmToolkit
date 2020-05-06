using System;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestNavigationDispatcherEntryListener : INavigationDispatcherEntryListener
    {
        #region Properties

        public Action<INavigationDispatcher, INavigationEntry, INavigationContext?>? OnNavigationEntryAdded { get; set; }

        public Action<INavigationDispatcher, INavigationEntry, INavigationContext?>? OnNavigationEntryUpdated { get; set; }

        public Action<INavigationDispatcher, INavigationEntry, INavigationContext?>? OnNavigationEntryRemoved { get; set; }

        #endregion

        #region Implementation of interfaces

        void INavigationDispatcherEntryListener.OnNavigationEntryAdded(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            OnNavigationEntryAdded?.Invoke(navigationDispatcher, navigationEntry, navigationContext);
        }

        void INavigationDispatcherEntryListener.OnNavigationEntryUpdated(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            OnNavigationEntryUpdated?.Invoke(navigationDispatcher, navigationEntry, navigationContext);
        }

        void INavigationDispatcherEntryListener.OnNavigationEntryRemoved(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext)
        {
            OnNavigationEntryRemoved?.Invoke(navigationDispatcher, navigationEntry, navigationContext);
        }

        #endregion
    }
}