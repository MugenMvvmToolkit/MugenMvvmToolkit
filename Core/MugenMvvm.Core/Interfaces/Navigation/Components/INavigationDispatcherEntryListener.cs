using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationDispatcherEntryListener : IComponent<INavigationDispatcher>
    {
        void OnNavigationEntryAdded(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext);

        void OnNavigationEntryUpdated(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext);

        void OnNavigationEntryRemoved(INavigationDispatcher dispatcher, INavigationEntry navigationEntry, INavigationContext? navigationContext);
    }
}