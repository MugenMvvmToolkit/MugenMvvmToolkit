using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationDispatcherEntryListener : IComponent<INavigationDispatcher>
    {
        void OnNavigationEntryAdded(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext);

        void OnNavigationEntryUpdated(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext);

        void OnNavigationEntryRemoved(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext);
    }
}