namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigatingCallback
    {
        void OnNavigatingFrom(object? toTarget, INavigationContext navigationContext);

        void OnNavigatingTo(object? fromTarget, INavigationContext navigationContext);
    }
}