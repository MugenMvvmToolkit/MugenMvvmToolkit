namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IRestorationViewModelPresenterResult : IChildViewModelPresenterResult
    {
        bool IsRestored { get; }
    }
}