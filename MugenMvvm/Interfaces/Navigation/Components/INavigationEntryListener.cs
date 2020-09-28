using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationEntryListener : IComponent<INavigationDispatcher>
    {
        void OnNavigationEntryAdded(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo);

        void OnNavigationEntryUpdated(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo);

        void OnNavigationEntryRemoved(INavigationDispatcher navigationDispatcher, INavigationEntry navigationEntry, IHasNavigationInfo? navigationInfo);
    }
}