namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigatingCallback
    {
        void OnNavigatingFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget);

        void OnNavigatingTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget);
    }
}