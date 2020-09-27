namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigatedCallback
    {
        void OnNavigatedFrom(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget);

        void OnNavigatedTo(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget);
    }
}