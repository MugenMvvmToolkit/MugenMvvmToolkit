namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IClosingViewModelPresenterResult : IChildViewModelPresenterResult
    {
        INavigationCallback<bool> ClosingCallback { get; }
    }
}