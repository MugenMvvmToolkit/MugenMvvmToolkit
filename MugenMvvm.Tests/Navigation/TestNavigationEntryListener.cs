using System;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationEntryListener : INavigationEntryListener
    {
        public Action<INavigationDispatcher, INavigationEntry, IHasNavigationInfo?>? OnNavigationEntryAdded { get; set; }

        public Action<INavigationDispatcher, INavigationEntry, IHasNavigationInfo?>? OnNavigationEntryUpdated { get; set; }

        public Action<INavigationDispatcher, INavigationEntry, IHasNavigationInfo?>? OnNavigationEntryRemoved { get; set; }

        void INavigationEntryListener.OnNavigationEntryAdded(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo) =>
            OnNavigationEntryAdded?.Invoke(navigationDispatcher, navigationEntry, navigationInfo);

        void INavigationEntryListener.OnNavigationEntryUpdated(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo) =>
            OnNavigationEntryUpdated?.Invoke(navigationDispatcher, navigationEntry, navigationInfo);

        void INavigationEntryListener.OnNavigationEntryRemoved(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo) =>
            OnNavigationEntryRemoved?.Invoke(navigationDispatcher, navigationEntry, navigationInfo);
    }
}