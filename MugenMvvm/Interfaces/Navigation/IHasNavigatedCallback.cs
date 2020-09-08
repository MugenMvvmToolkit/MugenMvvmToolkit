namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigatedCallback
    {
        void OnNavigatedFrom(object? toTarget, INavigationContext navigationContext);

        void OnNavigatedTo(object? fromTarget, INavigationContext navigationContext);
    }
}