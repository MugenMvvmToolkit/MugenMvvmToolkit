namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigatingHandler
    {
        void OnNavigatingFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget);

        void OnNavigatingTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget);
    }
}