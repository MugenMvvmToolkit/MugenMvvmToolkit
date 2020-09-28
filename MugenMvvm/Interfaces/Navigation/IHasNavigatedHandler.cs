namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigatedHandler
    {
        void OnNavigatedFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget);

        void OnNavigatedTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget);
    }
}