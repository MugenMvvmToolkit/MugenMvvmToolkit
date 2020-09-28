namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasCloseNavigationHandler
    {
        void OnClosing(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);

        void OnClosed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}