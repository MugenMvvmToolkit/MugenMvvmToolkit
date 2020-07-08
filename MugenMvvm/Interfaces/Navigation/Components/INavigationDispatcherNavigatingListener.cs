using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationDispatcherNavigatingListener : IComponent<INavigationDispatcher>
    {
        void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}