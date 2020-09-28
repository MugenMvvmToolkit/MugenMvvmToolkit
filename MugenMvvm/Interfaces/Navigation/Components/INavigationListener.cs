using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationListener : IComponent<INavigationDispatcher>
    {
        void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}